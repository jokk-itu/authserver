using AuthServer.Authorize.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;

namespace AuthServer.Authorize;

internal class AuthorizeInteractionService : IAuthorizeInteractionService
{
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IAuthorizeUserAccessor _userAccessor;
    private readonly IAuthenticatedUserAccessor _authenticatedUserAccessor;
    private readonly IAuthorizationGrantRepository _authorizationGrantRepository;
    private readonly IConsentGrantRepository _consentGrantRepository;
    private readonly ICachedClientStore _cachedClientStore;

    public AuthorizeInteractionService(
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IAuthorizeUserAccessor userAccessor,
        IAuthenticatedUserAccessor authenticatedUserAccessor,
        IAuthorizationGrantRepository authorizationGrantRepository,
        IConsentGrantRepository consentGrantRepository,
        ICachedClientStore cachedClientStore)
    {
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _userAccessor = userAccessor;
        _authenticatedUserAccessor = authenticatedUserAccessor;
        _authorizationGrantRepository = authorizationGrantRepository;
        _consentGrantRepository = consentGrantRepository;
        _cachedClientStore = cachedClientStore;
    }

    /// <inheritdoc/>
    public async Task<string> GetPrompt(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        // user was redirected for interaction
        var authorizeUser = _userAccessor.TryGetUser();
        if (authorizeUser is not null)
        {
            return await GetPromptFromInteraction(authorizeUser.SubjectIdentifier, authorizeRequest, cancellationToken);
        }

        /*
         client provided prompt overrides automatically deducing prompt.
         none is not checked, as that requires further validating session.
         */
        if (authorizeRequest.Prompt is PromptConstants.Login or PromptConstants.Consent or PromptConstants.SelectAccount)
        {
            return authorizeRequest.Prompt;
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
                return PromptConstants.Login;
            case > 1:
                return PromptConstants.SelectAccount;
            default:
                var authenticatedUser = (await _authenticatedUserAccessor.GetAuthenticatedUser())!;
                return await GetPromptFromCookie(authenticatedUser.SubjectIdentifier, authorizeRequest, cancellationToken);
        }
    }

    private async Task<string> GetPromptFromInteraction(string subjectIdentifier, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
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
            return PromptConstants.None;
        }

        var consentedScope = await _consentGrantRepository.GetConsentedScope(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken);
        if (authorizeRequest.Scope.ExceptAny(consentedScope))
        {
            return PromptConstants.Consent;
        }

        return PromptConstants.None;
    }

    private async Task<string> GetPromptFromCookie(string subjectIdentifier, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        var authorizationGrant = await _authorizationGrantRepository.GetActiveAuthorizationGrant(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken);
        if (authorizationGrant is null)
        {
            return PromptConstants.Login;
        }

        return await GetPromptSilent(authorizationGrant, authorizeRequest, subjectIdentifier, cancellationToken);
    }

    private async Task<string> GetPromptFromIdToken(string subjectIdentifier, string authorizationGrantId, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        var authorizationGrant = await _authorizationGrantRepository.GetActiveAuthorizationGrant(authorizationGrantId, cancellationToken);
        if (authorizationGrant is null)
        {
            return PromptConstants.Login;
        }

        return await GetPromptSilent(authorizationGrant, authorizeRequest, subjectIdentifier, cancellationToken);
    }

    private async Task<string> GetPromptSilent(AuthorizationGrant authorizationGrant, AuthorizeRequest authorizeRequest, string subjectIdentifier, CancellationToken cancellationToken)
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
            _userAccessor.SetUser(new AuthorizeUser(subjectIdentifier));
            return PromptConstants.None;
        }

        var consentedScope = await _consentGrantRepository.GetConsentedScope(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken);
        if (authorizeRequest.Scope.ExceptAny(consentedScope))
        {
            return PromptConstants.Consent;
        }

        _userAccessor.SetUser(new AuthorizeUser(subjectIdentifier));
        return PromptConstants.None;
    }

    private async Task<string?> GetPromptAcr(AuthorizationGrant authorizationGrant, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        var performedAuthenticationContextReference = authorizationGrant.AuthenticationContextReference.Name;
        var defaultAuthenticationContextReferences = (await _cachedClientStore.Get(authorizeRequest.ClientId!, cancellationToken))!.DefaultAcrValues;

        if (authorizeRequest.AcrValues.Count != 0 && !authorizeRequest.AcrValues.Contains(performedAuthenticationContextReference))
        {
            return PromptConstants.Login;
        }

        if (defaultAuthenticationContextReferences.Count != 0 && !defaultAuthenticationContextReferences.Contains(performedAuthenticationContextReference))
        {
            return PromptConstants.Login;
        }

        return null;
    }

    private static string? GetPromptMaxAge(AuthorizationGrant authorizationGrant, AuthorizeRequest authorizeRequest)
    {
        var hasMaxAge = int.TryParse(authorizeRequest.MaxAge, out var parsedMaxAge);
        var maxAge = hasMaxAge ? parsedMaxAge : authorizationGrant.Client.DefaultMaxAge;

        if (maxAge is not null && authorizationGrant.AuthTime.AddSeconds(maxAge.Value) < DateTime.UtcNow)
        {
            return PromptConstants.Login;
        }

        return null;
    }
}