using AuthServer.Cache.Abstractions;
using AuthServer.Cache.Entities;
using AuthServer.Cache.Exceptions;
using AuthServer.Core;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthServer.Cache;
internal class CachedClientStore : ICachedClientStore
{
    private readonly IDistributedCache _distributedCache;
    private readonly AuthorizationDbContext _identityContext;
    private readonly ILogger<CachedClientStore> _logger;
    private readonly Dictionary<string, CachedClient> _internalCache = [];

    public CachedClientStore(
        IDistributedCache distributedCache,
        AuthorizationDbContext identityContext,
        ILogger<CachedClientStore> logger)
    {
        _distributedCache = distributedCache;
        _identityContext = identityContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<CachedClient?> TryGet(string entityId, CancellationToken cancellationToken)
    {
        var isInternalCacheHit = _internalCache.TryGetValue($"Client#{entityId}", out var internalCachedClient);
        if (isInternalCacheHit)
        {
            return internalCachedClient!;
        }

        _logger.LogDebug("InternalCache miss for id {ClientId}", entityId);

        var cachedClient = await _distributedCache.Get<CachedClient>($"Client#{entityId}", cancellationToken);
        if (cachedClient is not null)
        {
            return cachedClient;
        }

        _logger.LogInformation("Cache miss for id {ClientId}", entityId);
        return await Add(entityId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CachedClient> Get(string entityId, CancellationToken cancellationToken)
    {
        var cachedClient = await TryGet(entityId, cancellationToken);
        return cachedClient ?? throw new ClientNotFoundException(entityId);
    }

    /// <inheritdoc/>
    public async Task Delete(string entityId, CancellationToken cancellationToken)
    {
        _internalCache.Remove($"Client#{entityId}");
        await _distributedCache.Delete($"Client#{entityId}", cancellationToken);
    }

    internal async Task<CachedClient?> Add(string entityId, CancellationToken cancellationToken)
    {
        var client = await _identityContext
            .Set<Client>()
            .Where(c => c.Id == entityId)
            .Include(c => c.Scopes)
            .Include(c => c.GrantTypes)
            .Include(c => c.ResponseTypes)
            .Include(c => c.PostLogoutRedirectUris)
            .Include(c => c.RedirectUris)
            .Include(c => c.RequestUris)
            .Include(x => x.ClientAuthenticationContextReferences)
            .ThenInclude(x => x.AuthenticationContextReference)
            .SingleOrDefaultAsync(cancellationToken);

        if (client is null)
        {
            _logger.LogInformation("Client {ClientId} does not exist", entityId);
            return null;
        }

        var cachedClient = new CachedClient
        {
            Id = client.Id,
            Name = client.Name,
            SecretHash = client.SecretHash,
            SecretExpiresAt = client.SecretExpiresAt,
            SecretExpiration = client.SecretExpiration,
            AccessTokenExpiration = client.AccessTokenExpiration,
            RefreshTokenExpiration = client.RefreshTokenExpiration,
            AuthorizationCodeExpiration = client.AuthorizationCodeExpiration,
            JwksExpiration = client.JwksExpiration,
            RequestUriExpiration = client.RequestUriExpiration,
            JwksUri = client.JwksUri,
            Jwks = client.Jwks,
            JwksExpiresAt = client.JwksExpiresAt,
            TosUri = client.TosUri,
            PolicyUri = client.PolicyUri,
            ClientUri = client.ClientUri,
            LogoUri = client.LogoUri,
            InitiateLoginUri = client.InitiateLoginUri,
            BackchannelLogoutUri = client.BackchannelLogoutUri,
            RequireReferenceToken = client.RequireReferenceToken,
            RequireConsent = client.RequireConsent,
            RequireSignedRequestObject = client.RequireSignedRequestObject,
            RequirePushedAuthorizationRequests = client.RequirePushedAuthorizationRequests,
            DefaultMaxAge = client.DefaultMaxAge,
            ApplicationType = client.ApplicationType,
            TokenEndpointAuthMethod = client.TokenEndpointAuthMethod,
            SubjectType = client.SubjectType,
            TokenEndpointAuthSigningAlg = client.TokenEndpointAuthSigningAlg,
            UserinfoEncryptedResponseEnc = client.UserinfoEncryptedResponseEnc,
            UserinfoEncryptedResponseAlg = client.UserinfoEncryptedResponseAlg,
            UserinfoSignedResponseAlg = client.UserinfoSignedResponseAlg,
            RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
            RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
            RequestObjectSigningAlg = client.RequestObjectSigningAlg,
            IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
            IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
            IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
            Scopes = client.Scopes.Select(s => s.Name).ToList(),
            GrantTypes = client.GrantTypes.Select(gt => gt.Name).ToList(),
            ResponseTypes = client.ResponseTypes.Select(rt => rt.Name).ToList(),
            PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(r => r.Uri).ToList(),
            RedirectUris = client.RedirectUris.Select(r => r.Uri).ToList(),
            RequestUris = client.RequestUris.Select(r => r.Uri).ToList(),
            DefaultAcrValues = client.ClientAuthenticationContextReferences
                .OrderBy(x => x.Order)
                .Select(x => x.AuthenticationContextReference)
                .Select(x => x.Name)
                .ToList()
        };

        _internalCache.Add($"Client#{entityId}", cachedClient);
        await _distributedCache.Add($"Client#{entityId}", cachedClient, null, cancellationToken);

        return cachedClient;
    }
}