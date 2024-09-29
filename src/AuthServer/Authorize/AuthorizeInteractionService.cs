using System.Text.Json;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
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

    public AuthorizeInteractionService(
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IAuthorizeUserAccessor userAccessor,
        IAuthenticatedUserAccessor authenticatedUserAccessor,
        IAuthorizationGrantRepository authorizationGrantRepository,
        IConsentGrantRepository consentGrantRepository)
    {
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _userAccessor = userAccessor;
        _authenticatedUserAccessor = authenticatedUserAccessor;
        _authorizationGrantRepository = authorizationGrantRepository;
        _consentGrantRepository = consentGrantRepository;
    }

    /// <inheritdoc/>
    public async Task<string> GetPrompt(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        // user was redirected for interaction
        var authorizeUser = _userAccessor.TryGetUser();
        if (authorizeUser is not null)
        {
            return await GetPromptFromInteraction(authorizeUser.SubjectIdentifier, authorizeRequest, authorizeUser.Amr, cancellationToken);
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
            var amr = JsonSerializer.Deserialize<IEnumerable<string>>(decodedIdToken.GetClaim(ClaimNameConstants.Amr).Value)!;
            return await GetPrompt(decodedIdToken.Subject, authorizeRequest, amr, cancellationToken);
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
                return await GetPrompt(authenticatedUser.SubjectIdentifier, authorizeRequest, authenticatedUser.Amr, cancellationToken);
        }
    }

    private async Task<string> GetPromptFromInteraction(string subjectIdentifier, AuthorizeRequest authorizeRequest, IEnumerable<string> amr, CancellationToken cancellationToken)
    {
        var authorizationGrant = (await _authorizationGrantRepository.GetActiveAuthorizationGrant(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken))!;
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

    private async Task<string> GetPrompt(string subjectIdentifier, AuthorizeRequest authorizeRequest, IEnumerable<string> amr, CancellationToken cancellationToken)
    {
        var authorizationGrant = await _authorizationGrantRepository.GetActiveAuthorizationGrant(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken);
        if (authorizationGrant is null)
        {
            return PromptConstants.Login;
        }

        if (!authorizationGrant.Client.RequireConsent)
        {
            _userAccessor.SetUser(new AuthorizeUser(subjectIdentifier, amr));
            return PromptConstants.None;
        }

        var consentedScope = await _consentGrantRepository.GetConsentedScope(subjectIdentifier, authorizeRequest.ClientId!, cancellationToken);
        if (authorizeRequest.Scope.ExceptAny(consentedScope))
        {
            return PromptConstants.Consent;
        }

        _userAccessor.SetUser(new AuthorizeUser(subjectIdentifier, amr));
        return PromptConstants.None;
    }
}