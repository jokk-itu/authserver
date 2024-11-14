using AuthServer.Authentication.Abstractions;
using AuthServer.Authentication.Exceptions;
using AuthServer.Cache.Abstractions;
using AuthServer.Core;
using AuthServer.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Authentication;
internal class ClientJwkService : IClientJwkService
{
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthorizationDbContext _identityContext;
    private readonly ILogger<ClientJwkService> _logger;

    public ClientJwkService(
        ICachedClientStore cachedClientStore,
        IHttpClientFactory httpClientFactory,
        AuthorizationDbContext identityContext,
        ILogger<ClientJwkService> logger)
    {
        _cachedClientStore = cachedClientStore;
        _httpClientFactory = httpClientFactory;
        _identityContext = identityContext;
        _logger = logger;
    }

    /// <inheritdoc/>
	public async Task<JsonWebKey?> GetEncryptionKey(string clientId, CancellationToken cancellationToken) =>
        (await GetKeys(clientId, JsonWebKeyUseNames.Enc, cancellationToken)).FirstOrDefault();

    /// <inheritdoc/>
	public async Task<IEnumerable<JsonWebKey>> GetSigningKeys(string clientId, CancellationToken cancellationToken) =>
        await GetKeys(clientId, JsonWebKeyUseNames.Sig, cancellationToken);

    /// <inheritdoc/>
	public async Task<IEnumerable<JsonWebKey>> GetKeys(string clientId, string use, CancellationToken cancellationToken)
    {
        var cachedClient = await _cachedClientStore.Get(clientId, cancellationToken);

        if (string.IsNullOrWhiteSpace(cachedClient.Jwks) && string.IsNullOrWhiteSpace(cachedClient.JwksUri))
        {
            return [];
        }

        var useJwks = cachedClient.JwksExpiresAt is null || DateTime.UtcNow < cachedClient.JwksExpiresAt;
        if (useJwks)
        {
            return JsonWebKeySet.Create(cachedClient.Jwks).Keys.Where(k => k.Use == use);
        }
        
        if (cachedClient.JwksUri is null)
        {
            return [];
        }

        _logger.LogDebug("Refreshing jwks for client {ClientId}", clientId);
        var jwks = await RefreshJwks(clientId, cachedClient.JwksUri!, cancellationToken);
        var jsonWebKeySet = JsonWebKeySet.Create(jwks);
        if (jsonWebKeySet.Keys.Any(x => x.HasPrivateKey))
        {
            _logger.LogWarning("Client {ClientId} response contains private keys", clientId);
            return [];
        }

        var client = (await _identityContext.FindAsync<Client>([clientId], cancellationToken))!;
        client.Jwks = jwks;
        client.JwksExpiresAt = DateTime.UtcNow.AddSeconds(client.JwksExpiration!.Value);

        return jsonWebKeySet.Keys.Where(k => k.Use == use);
    }

    /// <inheritdoc/>
    public async Task<string?> GetJwks(string jwksUri, CancellationToken cancellationToken)
    {
	    _logger.LogDebug("Initial fetch of jwks using uri {JwksUri}", jwksUri);
	    try
	    {
			return await RefreshJwks(null, jwksUri, cancellationToken);
	    }
	    catch (ClientJwkRefreshException e)
	    {
            _logger.LogError(e, "Unexpected error occurred during initial fetch of jwks {JwksUri}", jwksUri);
		    return null;
	    }
    }

    private async Task<string> RefreshJwks(string? clientId, string jwksUri, CancellationToken cancellationToken)
    {
        // TODO implement a Timeout to reduce Denial-Of-Service attacks
        // TODO implement retry delegate handler (5XX and 429)
        try
        {
            using var httpClient = _httpClientFactory.CreateClient(HttpClientNameConstants.Client);
            var response = await httpClient.GetAsync(jwksUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception e)
        {
            throw new ClientJwkRefreshException($"Unexpected error occurred during refreshing jwks for client '{clientId}'", e);
        }
    }
}