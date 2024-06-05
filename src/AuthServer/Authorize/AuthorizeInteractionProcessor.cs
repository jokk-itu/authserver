using AuthServer.Authorize.Abstract;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.TokenDecoders;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Authorize;

internal class AuthorizeInteractionProcessor : IAuthorizeInteractionProcessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IUserAccessor _userAccessor;

    public AuthorizeInteractionProcessor(
        IHttpContextAccessor httpContextAccessor,
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IUserAccessor userAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _userAccessor = userAccessor;
    }

    /// <inheritdoc/>
    public async Task<string> ProcessForInteraction(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        // covers the scenario where the user was redirected for interaction
        var user = _userAccessor.TryGetUser();
        if (user is not null)
        {
            return await GetPrompt(user.SubjectIdentifier, authorizeRequest, cancellationToken);
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
            return await GetPrompt(decodedIdToken.Subject, authorizeRequest, cancellationToken);
        }

        var users = _httpContextAccessor.HttpContext?.User?.Identities ?? [];
        var activeUsers = users.Where(x => x.IsAuthenticated).ToList() ?? [];
        switch (activeUsers.Count)
        {
            case 0:
                return PromptConstants.Login;
            case > 1:
                return PromptConstants.SelectAccount;
            default:
                var subjectIdentifier = activeUsers
                    .Single().Claims
                    .Single(x => x.Type == ClaimNameConstants.Sub).Value;

                return await GetPrompt(subjectIdentifier, authorizeRequest, cancellationToken);
        }
    }

    private async Task<string> GetPrompt(string subjectIdentifier, AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        var authorizationGrant = await _identityContext
            .Set<AuthorizationGrant>()
            .Include(x => x.Client)
            .Where(x => x.Client.Id == authorizeRequest.ClientId)
            .Where(x => x.Session.PublicSubjectIdentifier.Id == subjectIdentifier)
            .Where(AuthorizationGrant.IsMaxAgeValid)
            .Where(x => x.Session.RevokedAt == null)
            .SingleOrDefaultAsync(cancellationToken);

        if (authorizationGrant is null)
        {
            return PromptConstants.Login;
        }

        if (!authorizationGrant.Client.RequireConsent)
        {
            _userAccessor.TrySetUser(new User(subjectIdentifier, []));
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

        if (consentGrant.ConsentedScopes.Select(x => x.Name).Except(authorizeRequest.Scope).Any())
        {
            return PromptConstants.Consent;
        }

        _userAccessor.TrySetUser(new User(subjectIdentifier, []));
        return PromptConstants.None;
    }
}