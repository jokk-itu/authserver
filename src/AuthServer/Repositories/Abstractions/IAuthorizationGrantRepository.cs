using AuthServer.Entities;

namespace AuthServer.Repositories.Abstractions;

internal interface IAuthorizationGrantRepository
{
    /// <summary>
    /// Creates a new grant and revoked the previous grant, if it exists.
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="maxAge"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizationGrant> CreateAuthorizationGrant(string subjectIdentifier, string clientId, long? maxAge, CancellationToken cancellationToken);
}