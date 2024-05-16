using AuthServer.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Cache;

internal class TokenReplayCache : ITokenReplayCache
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<TokenReplayCache> _logger;

    public TokenReplayCache(
        IDistributedCache distributedCache,
        ILogger<TokenReplayCache> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
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