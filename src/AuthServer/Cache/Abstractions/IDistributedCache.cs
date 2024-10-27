namespace AuthServer.Cache.Abstractions;

/// <summary>
/// Must be a distributed cache, if using multiple server architecture to ensure reliable data.
/// </summary>
public interface IDistributedCache
{
    Task<T?> Get<T>(string key, CancellationToken cancellationToken) where T : class;
    Task Add<T>(string key, T entity, DateTime? expiresOn, CancellationToken cancellationToken) where T : class;
    Task Delete(string key, CancellationToken cancellationToken);
}