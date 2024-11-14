using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthServer.Repositories;

internal class AuthorizationGrantRepository : IAuthorizationGrantRepository
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ILogger<AuthorizationDbContext> _logger;

    public AuthorizationGrantRepository(
        AuthorizationDbContext identityContext,
        ILogger<AuthorizationDbContext> logger)
    {
        _identityContext = identityContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AuthorizationGrant> CreateAuthorizationGrant(
        string subjectIdentifier,
        string clientId,
        string authenticationContextReference,
        IReadOnlyCollection<string> authenticationMethodReferences,
        CancellationToken cancellationToken)
    {
        var session = await GetSession(subjectIdentifier, clientId, cancellationToken);
        var client = (await _identityContext.FindAsync<Client>([clientId], cancellationToken))!;
        var subject = await GetSubject(subjectIdentifier, clientId, cancellationToken);
        var acr = await GetAuthenticationContextReference(authenticationContextReference, cancellationToken);
        var amr = await GetAuthenticationMethodReferences(authenticationMethodReferences, cancellationToken);

        var newGrant = new AuthorizationGrant(session, client, subject, acr)
        {
            AuthenticationMethodReferences = amr
        };
        await _identityContext.AddAsync(newGrant, cancellationToken);
        await _identityContext.SaveChangesAsync(cancellationToken);
        return newGrant;
    }

    /// <inheritdoc/>
    public async Task<AuthorizationGrant?> GetActiveAuthorizationGrant(string subjectIdentifier, string clientId,
        CancellationToken cancellationToken)
    {
        return await _identityContext
            .Set<AuthorizationGrant>()
            .Include(x => x.AuthenticationContextReference)
            .Include(x => x.Client)
            .Where(AuthorizationGrant.IsActive)
            .Where(x => x.Client.Id == clientId)
            .Where(x => x.Session.RevokedAt == null)
            .Where(x => x.Session.SubjectIdentifier.Id == subjectIdentifier)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AuthorizationGrant?> GetActiveAuthorizationGrant(string authorizationGrantId,
        CancellationToken cancellationToken)
    {
        return await _identityContext
            .Set<AuthorizationGrant>()
            .Include(x => x.AuthenticationContextReference)
            .Where(AuthorizationGrant.IsActive)
            .Where(x => x.Session.RevokedAt == null)
            .Where(x => x.Id == authorizationGrantId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RevokeGrant(string authorizationGrantId, CancellationToken cancellationToken)
    {
        var affectedTokens = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(ag => ag.Id == authorizationGrantId)
            .SelectMany(g => g.GrantTokens)
            .Where(Token.IsActive)
            .ExecuteUpdateAsync(
                propertyCall => propertyCall.SetProperty(gt => gt.RevokedAt, DateTime.UtcNow),
                cancellationToken);

        var authorizationGrant = (await _identityContext.Set<AuthorizationGrant>().FindAsync([authorizationGrantId], cancellationToken))!;
        authorizationGrant.Revoke();

        _logger.LogInformation(
            "Revoked AuthorizationGrant {AuthorizationGrantId} and Tokens {AffectedTokens}",
            authorizationGrantId,
            affectedTokens);
    }

    private async Task<List<AuthenticationMethodReference>> GetAuthenticationMethodReferences(IReadOnlyCollection<string> authenticationMethodReferences, CancellationToken cancellationToken)
    {
        return await _identityContext
            .Set<AuthenticationMethodReference>()
            .Where(x => authenticationMethodReferences.Contains(x.Name))
            .ToListAsync(cancellationToken);
    }

    private async Task<AuthenticationContextReference> GetAuthenticationContextReference(string authenticationContextReference, CancellationToken cancellationToken)
    {
        return await _identityContext
            .Set<AuthenticationContextReference>()
            .Where(x => x.Name == authenticationContextReference)
            .SingleAsync(cancellationToken);
    }

    private async Task<Session> GetSession(string subjectIdentifier, string clientId,
        CancellationToken cancellationToken)
    {
        var session = await _identityContext
            .Set<Session>()
            .Include(x => x.SubjectIdentifier)
            .Where(x => x.SubjectIdentifier.Id == subjectIdentifier)
            .Where(Session.IsActive)
            .SingleOrDefaultAsync(cancellationToken);

        if (session is not null)
        {
            await RevokePreviousAuthorizationGrant(subjectIdentifier, clientId, cancellationToken);
        }

        var publicSubjectIdentifier = (session?.SubjectIdentifier ??
                                       await _identityContext.FindAsync<SubjectIdentifier>([subjectIdentifier],
                                           cancellationToken))!;

        session ??= new Session(publicSubjectIdentifier);
        return session;
    }

    private async Task RevokePreviousAuthorizationGrant(string subjectIdentifier, string clientId,
        CancellationToken cancellationToken)
    {
        var authorizationGrant = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(AuthorizationGrant.IsActive)
            .Include(x => x.Client)
            .Where(x => x.Client.Id == clientId)
            .Where(x => x.Session.RevokedAt == null)
            .Where(x => x.Session.SubjectIdentifier.Id == subjectIdentifier)
            .SingleOrDefaultAsync(cancellationToken);

        authorizationGrant?.Revoke();
    }

    private async Task<string> GetSubject(string subjectIdentifier, string clientId, CancellationToken cancellationToken)
    {
        var client = await _identityContext
            .Set<Client>()
            .Include(x => x.SectorIdentifier)
            .Where(x => x.Id == clientId)
            .SingleAsync(cancellationToken);

        if (client.SubjectType == SubjectType.Public)
        {
            return subjectIdentifier;
        }

        if (client.SubjectType == SubjectType.Pairwise)
        {
            return PairwiseSubjectHelper.GenerateSubject(client.SectorIdentifier!, subjectIdentifier);
        }

        throw new InvalidOperationException("SubjectType has invalid value");
    }
}