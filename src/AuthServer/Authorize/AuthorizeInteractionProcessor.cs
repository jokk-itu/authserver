using System.Text.Json;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Authorize;

internal class AuthorizeInteractionProcessor : IAuthorizeInteractionProcessor
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IAuthorizeUserAccessor _userAccessor;
    private readonly IAuthenticatedUserAccessor _authenticatedUserAccessor;

    public AuthorizeInteractionProcessor(
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IAuthorizeUserAccessor userAccessor,
        IAuthenticatedUserAccessor authenticatedUserAccessor)
    {
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _userAccessor = userAccessor;
        _authenticatedUserAccessor = authenticatedUserAccessor;
    }

    /// <inheritdoc/>
    public async Task<string> ProcessForInteraction(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        // covers the scenario where the user was redirected for interaction
        var authorizeUser = _userAccessor.TryGetUser();
        if (authorizeUser is not null)
        {
            return await GetPrompt(authorizeUser.SubjectIdentifier, authorizeRequest, authorizeUser.Amr, cancellationToken);
        }

        // client provided prompt overrides automatically deducing prompt
        if (PromptConstants.Prompts.Contains(authorizeRequest.Prompt))
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

        var authenticatedUsers =
            await _authenticatedUserAccessor.CountAuthenticatedUsers();

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

    private async Task<string> GetPrompt(string subjectIdentifier, AuthorizeRequest authorizeRequest, IEnumerable<string> amr, CancellationToken cancellationToken)
    {
        var authorizationGrant = await _identityContext
            .Set<AuthorizationGrant>()
            .Include(x => x.Client)
            .Where(x => x.Client.Id == authorizeRequest.ClientId)
            .Where(x => x.Session.PublicSubjectIdentifier.Id == subjectIdentifier)
            .Where(AuthorizationGrant.IsMaxAgeValid)
            .Where(x => x.Session.RevokedAt == null)
            .SingleOrDefaultAsync(cancellationToken);

        // TODO this does not make sense for login the first time, it only makes sense for the silent login flow
        if (authorizationGrant is null)
        {
            return PromptConstants.Login;
        }

        if (!authorizationGrant.Client.RequireConsent)
        {
            // only try set, because the user might already be set from interaction
            _userAccessor.TrySetUser(new AuthorizeUser(subjectIdentifier, amr));
            return PromptConstants.None;
        }

        var consentGrant = await _identityContext
            .Set<ConsentGrant>()
            .Include(x => x.ConsentedScopes)
            .Where(x => x.Client.Id == authorizeRequest.ClientId)
            .Where(x => x.PublicSubjectIdentifier.Id == subjectIdentifier)
            .SingleOrDefaultAsync(cancellationToken);

        if (consentGrant is null)
        {
            return PromptConstants.Consent;
        }

        if (authorizeRequest.Scope.ExceptAny(consentGrant.ConsentedScopes.Select(x => x.Name)))
        {
            return PromptConstants.Consent;
        }

        // only try set, because the user might already be set from the interaction
        _userAccessor.TrySetUser(new AuthorizeUser(subjectIdentifier, amr));
        return PromptConstants.None;
    }
}