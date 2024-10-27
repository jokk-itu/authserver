using System.Text.Json;
using AuthServer.Cache.Abstractions;

namespace AuthServer.Tests.Core;
public class InMemoryCache : IDistributedCache
{
    private readonly Dictionary<string, string> _cache = [];

    public virtual Task<T?> Get<T>(string key, CancellationToken cancellationToken) where T : class
    {
        return _cache.TryGetValue(key, out var entity)
            ? Task.FromResult(JsonSerializer.Deserialize<T>(entity))
            : Task.FromResult<T?>(null);
    }

    public virtual Task Add<T>(string key, T entity, DateTime? expiresOn, CancellationToken cancellationToken) where T : class
    {
        _cache.Add(key, JsonSerializer.Serialize(entity));
        return Task.CompletedTask;
    }

    public virtual Task Delete(string key, CancellationToken cancellationToken)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}