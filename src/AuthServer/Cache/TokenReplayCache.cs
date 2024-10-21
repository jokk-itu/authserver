using AuthServer.Cache.Abstractions;
using AuthServer.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Cache;

internal class TokenReplayCache : ITokenReplayCache
{
    private readonly IDistributedCache _distributedCache;

    public TokenReplayCache(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public bool TryAdd(string securityToken, DateTime expiresOn)
    {
        var key = securityToken.Sha256();
        _distributedCache.Add(key, securityToken, expiresOn, CancellationToken.None).GetAwaiter().GetResult();
        return true;
    }

    public bool TryFind(string securityToken)
    {
        var key = securityToken.Sha256();
        var entity = _distributedCache.Get<string>(key, CancellationToken.None).GetAwaiter().GetResult();
        return entity is not null;
    }
}