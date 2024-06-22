using System.Collections.Concurrent;
using AuthServer.Cache.Abstractions;
using AuthServer.Core;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthServer.Cache;

internal class EntityInMemoryCache : IEntityInMemoryCache, IDisposable
{
    private readonly ILogger<EntityInMemoryCache> _logger;
    private readonly IServiceScope _serviceScope;
    private readonly AuthorizationDbContext _authorizationDbContext;

    private readonly ConcurrentDictionary<string, Scope> _scopeCache = [];
    private readonly ConcurrentDictionary<string, GrantType> _grantTypeCache = [];
    private readonly ConcurrentDictionary<string, ResponseType> _responseTypeCache = [];

    public EntityInMemoryCache(
        IServiceProvider globalServiceProvider,
        ILogger<EntityInMemoryCache> logger)
    {
        _logger = logger;
        _serviceScope = globalServiceProvider.CreateScope();
        _authorizationDbContext = _serviceScope.ServiceProvider.GetRequiredService<AuthorizationDbContext>();
    }

    public async Task<Scope> GetScope(string scope, CancellationToken cancellationToken)
    {
        if(_scopeCache.TryGetValue(scope, out var cachedScope))
        {
            return cachedScope;
        }

        _logger.LogDebug("Cache miss for scope {Scope}", scope);

        var scopeEntity = await _authorizationDbContext
            .Set<Scope>()
            .AsNoTracking()
            .SingleAsync(s => s.Name == scope, cancellationToken);

        // This might be false if another concurrent Scope fetch is executing
        _scopeCache.TryAdd(scope, scopeEntity);

        return scopeEntity;
    }

    public async Task<GrantType> GetGrantType(string grantType, CancellationToken cancellationToken)
    {
        if (_grantTypeCache.TryGetValue(grantType, out var cachedGrantType))
        {
            return cachedGrantType;
        }

        _logger.LogDebug("Cache miss for GrantType {GrantType}", grantType);

        var grantTypeEntity = await _authorizationDbContext
            .Set<GrantType>()
            .AsNoTracking()
            .SingleAsync(gt => gt.Name == grantType, cancellationToken);

        // This might be false if another concurrent GrantType fetch is executing
        _grantTypeCache.TryAdd(grantType, grantTypeEntity);

        return grantTypeEntity;
    }

    public async Task<ResponseType> GetResponseType(string responseType, CancellationToken cancellationToken)
    {
        if (_responseTypeCache.TryGetValue(responseType, out var cachedResponseType))
        {
            return cachedResponseType;
        }

        _logger.LogDebug("Cache miss for ResponseType {ResponseType}", responseType);

        var responseTypeEntity = await _authorizationDbContext
            .Set<ResponseType>()
            .AsNoTracking()
            .SingleAsync(rt => rt.Name == responseType, cancellationToken);

        // This might be false if another concurrent ResponseType fetch is executing
        _responseTypeCache.TryAdd(responseType, responseTypeEntity);

        return responseTypeEntity;
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}