using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repositories;

internal class AuthorizationGrantRepository : IAuthorizationGrantRepository
{
    private readonly AuthorizationDbContext _identityContext;

    public AuthorizationGrantRepository(
        AuthorizationDbContext identityContext)
    {
        _identityContext = identityContext;
    }

    /// <inheritdoc/>
    public async Task<AuthorizationGrant> CreateAuthorizationGrant(string subjectIdentifier, string clientId,
        long? maxAge,
        CancellationToken cancellationToken)
    {
        var session = await GetSession(subjectIdentifier, clientId, cancellationToken);
        var client = (await _identityContext.FindAsync<Client>([clientId], cancellationToken))!;
        var subjectIdentifierForGrant =
            await GetSubjectIdentifierForGrant(subjectIdentifier, clientId, cancellationToken);

        maxAge ??= client.DefaultMaxAge;

        var newGrant = new AuthorizationGrant(session, client, subjectIdentifierForGrant, maxAge);
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
            .Include(x => x.Client)
            .ThenInclude(x => x.ClientAuthenticationContextReferences)
            .ThenInclude(x => x.AuthenticationContextReference)
            .Include(x => x.AuthenticationMethodReferences)
            .ThenInclude(x => x.AuthenticationContextReference)
            .Where(AuthorizationGrant.IsMaxAgeValid)
            .Where(x => x.Client.Id == clientId)
            .Where(x => x.Session.RevokedAt == null)
            .Where(x => x.Session.PublicSubjectIdentifier.Id == subjectIdentifier)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AuthorizationGrant?> GetActiveAuthorizationGrant(string authorizationGrantId,
        CancellationToken cancellationToken)
    {
        return await _identityContext
            .Set<AuthorizationGrant>()
            .Include(x => x.Client)
            .ThenInclude(x => x.ClientAuthenticationContextReferences)
            .ThenInclude(x => x.AuthenticationContextReference)
            .Include(x => x.AuthenticationMethodReferences)
            .ThenInclude(x => x.AuthenticationContextReference)
            .Where(AuthorizationGrant.IsMaxAgeValid)
            .Where(x => x.Session.RevokedAt == null)
            .Where(x => x.Id == authorizationGrantId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<List<AuthenticationMethodReference>> GetAuthenticationMethodReferences(IReadOnlyCollection<string> authenticationMethodReferences, CancellationToken cancellationToken)
    {
        return await _identityContext
            .Set<AuthenticationMethodReference>()
            .Where(x => authenticationMethodReferences.Contains(x.Name))
            .ToListAsync(cancellationToken);
    }

    private async Task<Session> GetSession(string subjectIdentifier, string clientId,
        CancellationToken cancellationToken)
    {
        var session = await _identityContext
            .Set<Session>()
            .Include(x => x.PublicSubjectIdentifier)
            .Where(x => x.PublicSubjectIdentifier.Id == subjectIdentifier)
            .Where(Session.IsActive)
            .SingleOrDefaultAsync(cancellationToken);

        if (session is not null)
        {
            await RevokePreviousGrant(subjectIdentifier, clientId, cancellationToken);
        }

        var publicSubjectIdentifier = (session?.PublicSubjectIdentifier ??
                                       await _identityContext.FindAsync<PublicSubjectIdentifier>([subjectIdentifier],
                                           cancellationToken))!;

        session ??= new Session(publicSubjectIdentifier);
        return session;
    }

    private async Task RevokePreviousGrant(string subjectIdentifier, string clientId,
        CancellationToken cancellationToken)
    {
        var grant = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(AuthorizationGrant.IsMaxAgeValid)
            .Include(x => x.Client)
            .Where(x => x.Client.Id == clientId)
            .Where(x => x.Session.RevokedAt == null)
            .Where(x => x.Session.PublicSubjectIdentifier.Id == subjectIdentifier)
            .SingleOrDefaultAsync(cancellationToken);

        grant?.Revoke();
    }

    private async Task<SubjectIdentifier> GetSubjectIdentifierForGrant(string subjectIdentifier, string clientId,
        CancellationToken cancellationToken)
    {
        var client = (await _identityContext.FindAsync<Client>([clientId], cancellationToken))!;
        if (client.SubjectType == SubjectType.Public)
        {
            return (await _identityContext.FindAsync<PublicSubjectIdentifier>([subjectIdentifier], cancellationToken))!;
        }

        if (client.SubjectType == SubjectType.Pairwise)
        {
            var pairwiseSubjectIdentifier = await _identityContext
                .Set<PublicSubjectIdentifier>()
                .Where(x => x.Id == subjectIdentifier)
                .SelectMany(x => x.PairwiseSubjectIdentifiers)
                .Where(x => x.Client.Id == clientId)
                .SingleOrDefaultAsync(cancellationToken);

            if (pairwiseSubjectIdentifier is not null)
            {
                return pairwiseSubjectIdentifier;
            }

            var publicSubjectIdentifier =
                (await _identityContext.FindAsync<PublicSubjectIdentifier>([subjectIdentifier], cancellationToken))!;
            pairwiseSubjectIdentifier = new PairwiseSubjectIdentifier(client, publicSubjectIdentifier);
            await _identityContext.AddAsync(pairwiseSubjectIdentifier, cancellationToken);
            return pairwiseSubjectIdentifier;
        }

        throw new InvalidOperationException("SubjectType has invalid value");
    }
}