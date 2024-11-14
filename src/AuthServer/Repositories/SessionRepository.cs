using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthServer.Repositories;

internal class SessionRepository : ISessionRepository
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly ILogger<SessionRepository> _logger;

    public SessionRepository(
        AuthorizationDbContext authorizationDbContext,
        ILogger<SessionRepository> logger)
    {
        _authorizationDbContext = authorizationDbContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task RevokeSession(string sessionId, CancellationToken cancellationToken)
    {
        var affectedTokens = await _authorizationDbContext
            .Set<AuthorizationGrant>()
            .Where(ag => ag.Session.Id == sessionId)
            .Where(AuthorizationGrant.IsActive)
            .SelectMany(g => g.GrantTokens)
            .Where(Token.IsActive)
            .ExecuteUpdateAsync(
                propertyCall => propertyCall.SetProperty(gt => gt.RevokedAt, DateTime.UtcNow),
                cancellationToken);

        var affectedGrants = await _authorizationDbContext
            .Set<AuthorizationGrant>()
            .Where(g => g.Session.Id == sessionId)
            .Where(AuthorizationGrant.IsActive)
            .ExecuteUpdateAsync(
                propertyCall => propertyCall.SetProperty(g => g.RevokedAt, DateTime.UtcNow),
                cancellationToken);

        var session = (await _authorizationDbContext.FindAsync<Session>([sessionId], cancellationToken))!;
        session?.Revoke();

        _logger.LogDebug(
            "Revoked Session {SessionId}, AuthorizationGrants {AffectedGrants} and Tokens {AffectedTokens}",
            sessionId,
            affectedGrants,
            affectedTokens);
    }
}