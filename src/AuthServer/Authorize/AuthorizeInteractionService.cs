using AuthServer.Authentication.Abstractions;
using AuthServer.Authorize.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.Logging;

namespace AuthServer.Authorize;

internal class AuthorizeInteractionService : IAuthorizeInteractionService
{
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IAuthorizeUserAccessor _userAccessor;
    private readonly IAuthenticatedUserAccessor _authenticatedUserAccessor;
    private readonly IAuthorizationGrantRepository _authorizationGrantRepository;
    private readonly IConsentGrantRepository _consentGrantRepository;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly ILogger<AuthorizeInteractionService> _logger;

    public AuthorizeInteractionService(
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IAuthorizeUserAccessor userAccessor,
        IAuthenticatedUserAccessor authenticatedUserAccessor,
        IAuthorizationGrantRepository authorizationGrantRepository,
        IConsentGrantRepository consentGrantRepository,
        ICachedClientStore cachedClientStore,
        ILogger<AuthorizeInteractionService> logger)
    {
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _userAccessor = userAccessor;
        _authenticatedUserAccessor = authenticatedUserAccessor;
        _authorizationGrantRepository = authorizationGrantRepository;
        _consentGrantRepository = consentGrantRepository;
        _cachedClientStore = cachedClientStore;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<InteractionResult> GetInteractionResult(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        // user was redirected for interaction
        var authorizeUser = _userAccessor.TryGetUser();
        if (authorizeUser is not null)
        {
            _logger.LogDebug("Deducing prompt from interaction with {@User}", authorizeUser);
            return await GetPromptFromInteraction(authorizeUser.SubjectIdentifier, authorizeRequest, cancellationToken);
        }

        /*
         client provided prompt overrides automatically deducing prompt.
         none is not checked, as that requires further validating session.
         */
        if (authorizeRequest.Prompt is PromptConstants.Login or PromptConstants.Consent or PromptConstants.SelectAccount)
        {
            _logger.LogDebug("Using prompt {Prompt} from request", authorizeRequest.Prompt);

            return authorizeRequest.Prompt switch
            {
                PromptConstants.Login => InteractionResult.LoginResult,
                PromptConstants.Consent => InteractionResult.ConsentResult,
                PromptConstants.SelectAccount => InteractionResult.SelectAccountResult,
                _ => throw new InvalidOperationException($"prompt {authorizeRequest.Prompt} is not supported")
            };
        }

        // id_token_hint overrides cookies, and only deduces prompt none, if validation succeeds
        if (!string.IsNullOrEmpty(authorizeRequest.IdTokenHint))
        {
            var decodedIdToken = await _serverIssuedTokenDecoder.Read(authorizeRequest.IdTokenHint);
            var subject = decodedIdToken.Subject;
            var grantId = decodedIdToken.GetClaim(ClaimNameConstants.GrantId).Value;
            return await GetPromptFromIdToken(subject, grantId, authorizeRequest, cancellationToken);
        }

        var authenticatedUsers = await _authenticatedUserAccessor.CountAuthenticatedUsers();
        switch (authenticatedUsers)
        {
            case 0:
                _logger.LogDebug("No authenticated users, deducing prompt {Prompt}", PromptConstants.Login);
                return InteractionResult.LoginResult;
            case > 1:
                _logger.LogDebug("Multiple authenticated users, deducing prompt {Prompt}", PromptConstants.SelectAccount);
                return InteractionResult.SelectAccountResult;
            default:
                var authenticatedUser = (await _authenticatedUserAccessor.GetAuthenticatedUser())!;
                _logger.LogDebug("Deducing Prompt from one authenticated user {@User}", authenticatedUser);
                return await GetPromptFromCookie(authenticatedUser.SubjectIdentifier, authorizeRequest, cancellationToken);
        }
    }

    private async Task<InteractionResult> GetPromptFromInteraction(string subjectIdentifier, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        var authorizationGrant = (await _authorizationGrantRepository.GetActiveAuthorizationGrant(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken))!;

        var maxAgePrompt = GetPromptMaxAge(authorizationGrant, authorizeRequest);
        if (maxAgePrompt is not null)
        {
            return maxAgePrompt;
        }

        var acrPrompt = await GetPromptAcr(authorizationGrant, authorizeRequest, cancellationToken);
        if (acrPrompt is not null)
        {
            return acrPrompt;
        }

        if (!authorizationGrant.Client.RequireConsent)
        {
            _logger.LogDebug("Client {ClientId} does not require consent, deducing prompt {Prompt}", authorizeRequest.ClientId, PromptConstants.None);
            return InteractionResult.Success(subjectIdentifier);
        }

        var consentedScope = await _consentGrantRepository.GetConsentedScope(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken);
        if (authorizeRequest.Scope.ExceptAny(consentedScope))
        {
            _logger.LogDebug("User has not granted consent to scope {@Scope}, deducing prompt {Prompt}", authorizeRequest.Scope.Except(consentedScope), PromptConstants.Consent);
            return InteractionResult.ConsentResult;
        }

        _logger.LogDebug("Deducing prompt {Prompt}", PromptConstants.None);
        return InteractionResult.Success(subjectIdentifier);
    }

    private async Task<InteractionResult> GetPromptFromCookie(string subjectIdentifier, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        var authorizationGrant = await _authorizationGrantRepository.GetActiveAuthorizationGrant(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken);
        if (authorizationGrant is null)
        {
            _logger.LogDebug("Grant has expired, used sub {SubjectIdentifier} and client id {ClientId}, deducing prompt {Prompt}", subjectIdentifier, authorizeRequest.ClientId!, PromptConstants.Login);
            return InteractionResult.LoginResult;
        }

        return await GetPromptSilent(authorizationGrant, authorizeRequest, subjectIdentifier, cancellationToken);
    }

    private async Task<InteractionResult> GetPromptFromIdToken(string subjectIdentifier, string authorizationGrantId, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        var authorizationGrant = await _authorizationGrantRepository.GetActiveAuthorizationGrant(authorizationGrantId, cancellationToken);
        if (authorizationGrant is null)
        {
            _logger.LogDebug("Grant {GrantId} has expired, deducing prompt {Prompt}", authorizationGrantId, PromptConstants.Login);
            return InteractionResult.LoginResult;
        }

        return await GetPromptSilent(authorizationGrant, authorizeRequest, subjectIdentifier, cancellationToken);
    }

    private async Task<InteractionResult> GetPromptSilent(AuthorizationGrant authorizationGrant, AuthorizeRequest authorizeRequest, string subjectIdentifier, CancellationToken cancellationToken)
    {
        var maxAgePrompt = GetPromptMaxAge(authorizationGrant, authorizeRequest);
        if (maxAgePrompt is not null)
        {
            return maxAgePrompt;
        }

        var acrPrompt = await GetPromptAcr(authorizationGrant, authorizeRequest, cancellationToken);
        if (acrPrompt is not null)
        {
            return acrPrompt;
        }

        if (!authorizationGrant.Client.RequireConsent)
        {
            _logger.LogDebug("Client {ClientId} does not require consent, deducing prompt {Prompt}", authorizeRequest.ClientId, PromptConstants.None);
            return InteractionResult.Success(subjectIdentifier);
        }

        var consentedScope = await _consentGrantRepository.GetConsentedScope(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken);
        if (authorizeRequest.Scope.ExceptAny(consentedScope))
        {
            _logger.LogDebug("User has not granted consent to scope {@Scope}, deducing prompt {Prompt}", authorizeRequest.Scope.Except(consentedScope), PromptConstants.Consent);
            return InteractionResult.ConsentResult;
        }

        _logger.LogDebug("Deducing prompt {Prompt}", PromptConstants.None);
        return InteractionResult.Success(subjectIdentifier);
    }

    private async Task<InteractionResult?> GetPromptAcr(AuthorizationGrant authorizationGrant, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        var performedAuthenticationContextReference = authorizationGrant.AuthenticationContextReference.Name;
        var defaultAuthenticationContextReferences = (await _cachedClientStore.Get(authorizeRequest.ClientId!, cancellationToken))!.DefaultAcrValues;

        if (authorizeRequest.AcrValues.Count != 0 && !authorizeRequest.AcrValues.Contains(performedAuthenticationContextReference))
        {
            _logger.LogDebug("Acr {@RequestedAcr} is not met, performed Acr {PerformedAcr}", authorizeRequest.AcrValues, performedAuthenticationContextReference);
            return InteractionResult.UnmetAuthenticationRequirementResult;
        }

        if (defaultAuthenticationContextReferences.Count != 0 && !defaultAuthenticationContextReferences.Contains(performedAuthenticationContextReference))
        {
            _logger.LogDebug("Acr {@DefaultAcr} is not met, performed Acr {PerformedAcr}", defaultAuthenticationContextReferences, performedAuthenticationContextReference);
            return InteractionResult.UnmetAuthenticationRequirementResult;
        }

        return null;
    }

    private InteractionResult? GetPromptMaxAge(AuthorizationGrant authorizationGrant, AuthorizeRequest authorizeRequest)
    {
        var hasMaxAge = int.TryParse(authorizeRequest.MaxAge, out var parsedMaxAge);
        var maxAge = hasMaxAge ? parsedMaxAge : authorizationGrant.Client.DefaultMaxAge;

        if (maxAge is not null && authorizationGrant.AuthTime.AddSeconds(maxAge.Value) < DateTime.UtcNow)
        {
            _logger.LogDebug("MaxAge {MaxAge} has been reached for grant {GrantId}, deducing prompt {Prompt}", maxAge, authorizationGrant.Id, PromptConstants.Login);
            return InteractionResult.LoginResult;
        }

        return null;
    }
}