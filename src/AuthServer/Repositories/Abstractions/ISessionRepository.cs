namespace AuthServer.Repositories.Abstractions;
internal interface ISessionRepository
{
    /// <summary>
    /// Revokes session if active, with all relations.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RevokeSession(string sessionId, CancellationToken cancellationToken);
}
