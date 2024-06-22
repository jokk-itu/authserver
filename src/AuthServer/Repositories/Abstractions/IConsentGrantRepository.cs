using AuthServer.Entities;

namespace AuthServer.Repositories.Abstractions;
internal interface IConsentGrantRepository
{
    /// <summary>
    /// Get all consented scopes from an active <see cref="ConsentGrant"/>.
    /// If empty, then consent grant is not active.
    /// </summary>
    /// <param name="publicSubjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<string>> GetConsentedScope(string publicSubjectIdentifier, string clientId, CancellationToken cancellationToken);
}