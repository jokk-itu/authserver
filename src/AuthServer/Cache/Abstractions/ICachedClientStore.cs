using AuthServer.Cache.Entities;
using AuthServer.Cache.Exceptions;

namespace AuthServer.Cache.Abstractions;
internal interface ICachedClientStore
{
    /// <summary>
    /// Fetches a cached entity of type <see cref="CachedClient"/>.
    /// If it results in a cache miss, it will automatically add it, and then return it.
    /// If the entityId does not exist, null is returned.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="CachedClient"/></returns>
    Task<CachedClient?> TryGet(string entityId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a cached entity of type <see cref="CachedClient"/>.
    /// If it results in a cache miss, it will automatically add it, and then return it.
    /// If the entityId an exception <exception cref="ClientNotFoundException"></exception> is thrown.
    /// </summary>
    /// <returns></returns>
    Task<CachedClient> Get(string entityId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a cached entity of type <see cref="CachedClient"/>.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Delete(string entityId, CancellationToken cancellationToken);
}