using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Repositories.Abstract;
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
    public async Task<AuthorizationGrant> CreateAuthorizationGrant(string subjectIdentifier, string clientId, long? maxAge,
        CancellationToken cancellationToken)
    {
        var session = await _identityContext
            .Set<Session>()
            .Include(x => x.PublicSubjectIdentifier)
            .Where(x => x.PublicSubjectIdentifier.Id == subjectIdentifier)
            .Where(x => x.RevokedAt == null)
            .SingleOrDefaultAsync(cancellationToken);

        var publicSubjectIdentifier = session?.PublicSubjectIdentifier
                                      ?? await _identityContext
                                          .Set<PublicSubjectIdentifier>()
                                          .Where(x => x.Id == subjectIdentifier)
                                          .SingleAsync(cancellationToken);

        session ??= new Session(publicSubjectIdentifier);

        var grant = await _identityContext
            .Set<AuthorizationGrant>()
            .Include(x => x.Client)
            .Where(x => x.Client.Id == clientId)
            .Where(x => x.Session.RevokedAt == null)
            .Where(x => x.Session.PublicSubjectIdentifier.Id == subjectIdentifier)
            .SingleOrDefaultAsync(cancellationToken);

        grant?.Revoke();

        var client = grant?.Client
                     ?? await _identityContext
                         .Set<Client>()
                         .Where(x => x.Id == clientId)
                         .SingleAsync(cancellationToken);

        var newGrant = new AuthorizationGrant(DateTime.UtcNow, session, client, publicSubjectIdentifier, maxAge);
        await _identityContext.AddAsync(newGrant, cancellationToken);
        return newGrant;
    }
}