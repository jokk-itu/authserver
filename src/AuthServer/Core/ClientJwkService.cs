﻿using AuthServer.Cache;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Exceptions;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Core;
internal class ClientJwkService : IClientJwkService
{
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IdentityContext _identityContext;
    private readonly ILogger<ClientJwkService> _logger;

    public ClientJwkService(
        ICachedClientStore cachedClientStore,
        IHttpClientFactory httpClientFactory,
        IdentityContext identityContext,
        ILogger<ClientJwkService> logger)
    {
        _cachedClientStore = cachedClientStore;
        _httpClientFactory = httpClientFactory;
        _identityContext = identityContext;
        _logger = logger;
    }

    public async Task<JsonWebKey?> GetEncryptionKey(string clientId, CancellationToken cancellationToken) =>
        (await GetKeys(clientId, JsonWebKeyUseNames.Enc, cancellationToken)).FirstOrDefault();

    public async Task<IEnumerable<JsonWebKey>> GetSigningKeys(string clientId, CancellationToken cancellationToken) =>
        await GetKeys(clientId, JsonWebKeyUseNames.Sig, cancellationToken);

    public async Task<IEnumerable<JsonWebKey>> GetKeys(string clientId, string use, CancellationToken cancellationToken)
    {
        var cachedClient = await _cachedClientStore.Get(clientId, cancellationToken);

        if (string.IsNullOrWhiteSpace(cachedClient.Jwks) && string.IsNullOrWhiteSpace(cachedClient.JwksUri))
        {
            return [];
        }

        var useJwks = cachedClient.JwksUri is null || DateTime.UtcNow < cachedClient.JwksExpiresAt;
        if (useJwks)
        {
            return JsonWebKeySet.Create(cachedClient.Jwks).Keys.Where(k => k.Use == use);
        }

        _logger.LogDebug("Refreshing jwks for client {ClientId}", clientId);
        var jwks = await RefreshJwks(clientId, cachedClient.JwksUri!, cancellationToken);
        // TODO verify they ONLY contain public keys
        var client = await _identityContext
            .Set<Client>()
            .SingleAsync(x => x.Id == clientId, cancellationToken);

        client.Jwks = jwks;
        client.JwksExpiresAt = DateTime.UtcNow.AddSeconds(client.JwksExpiration);

        // TODO implement CachedClient deletion as a database interceptor

        return JsonWebKeySet.Create(jwks).Keys.Where(k => k.Use == use);
    }

    private async Task<string> RefreshJwks(string clientId, string jwksUri, CancellationToken cancellationToken)
    {
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