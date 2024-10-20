using AuthServer.Entities;

namespace AuthServer.Repositories.Abstractions;
internal interface IConsentGrantRepository
{
    /// <summary>
    /// Get all consented scopes from an active <see cref="ConsentGrant"/>.
    /// If empty, then consent grant is not active.
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<string>> GetConsentedScope(string subjectIdentifier, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<string>> GetConsentedClaims(string subjectIdentifier, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="scope"></param>
    /// <param name="claims"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateOrUpdateConsentGrant(string subjectIdentifier, string clientId, IEnumerable<string> scope, IEnumerable<string> claims, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<ConsentGrant?> GetConsentGrant(string subjectIdentifier, string clientId, CancellationToken cancellationToken);
}