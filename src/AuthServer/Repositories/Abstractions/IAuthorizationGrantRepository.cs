using AuthServer.Entities;

namespace AuthServer.Repositories.Abstractions;

internal interface IAuthorizationGrantRepository
{
    /// <summary>
    /// Creates a new grant and revoked the previous grant, if it exists.
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="authenticationContextReference"></param>
    /// <param name="authenticationMethodReferences"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizationGrant> CreateAuthorizationGrant(string subjectIdentifier, string clientId, string authenticationContextReference, IReadOnlyCollection<string> authenticationMethodReferences, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizationGrant?> GetActiveAuthorizationGrant(string subjectIdentifier, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="authorizationGrantId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizationGrant?> GetActiveAuthorizationGrant(string authorizationGrantId, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes grant if active, with all relations.
    /// </summary>
    /// <param name="authorizationGrantId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RevokeGrant(string authorizationGrantId, CancellationToken cancellationToken);
}