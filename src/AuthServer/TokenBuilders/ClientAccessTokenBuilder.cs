using System.Diagnostics;
using System.Text.Json;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Metrics;
using AuthServer.Metrics.Abstractions;
using AuthServer.Options;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;

internal class ClientAccessTokenBuilder : ITokenBuilder<ClientAccessTokenArguments>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IMetricService _metricService;

    public ClientAccessTokenBuilder(
        AuthorizationDbContext identityContext,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IMetricService metricService)
    {
        _identityContext = identityContext;
        _jwksDocumentOptions = jwksDocumentOptions;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _metricService = metricService;
    }

    public async Task<string> BuildToken(ClientAccessTokenArguments arguments, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var client = (await _identityContext.FindAsync<Client>([arguments.ClientId], cancellationToken))!;
        if (client.RequireReferenceToken)
        {
            var referenceToken = await BuildReferenceToken(arguments, client);
            stopWatch.Stop();
            _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.AccessToken, TokenStructureTag.Reference);
            return referenceToken;
        }

        var jwt = BuildStructuredToken(arguments, client);
        stopWatch.Stop();
        _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.AccessToken, TokenStructureTag.Jwt);
        return jwt;
    }

    private string BuildStructuredToken(ClientAccessTokenArguments arguments, Client client)
    {
        var claims = new Dictionary<string, object>
        {
            { ClaimNameConstants.Jti, Guid.NewGuid() },
            { ClaimNameConstants.Scope, string.Join(' ', arguments.Scope) },
            { ClaimNameConstants.Aud, JsonSerializer.SerializeToElement(arguments.Resource) },
            { ClaimNameConstants.ClientId, arguments.ClientId },
            { ClaimNameConstants.Sub, arguments.ClientId }
        };

        var now = DateTime.UtcNow;
        var signingKey = _jwksDocumentOptions.Value.GetTokenSigningKey();
        var signingCredentials = new SigningCredentials(signingKey.Key, signingKey.Alg.GetDescription());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddSeconds(client.AccessTokenExpiration),
            NotBefore = now,
            Issuer = _discoveryDocumentOptions.Value.Issuer,
            SigningCredentials = signingCredentials,
            TokenType = TokenTypeHeaderConstants.AccessToken,
            Claims = claims
        };
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }

    private async Task<string> BuildReferenceToken(ClientAccessTokenArguments arguments, Client client)
    {
        var accessToken = new ClientAccessToken(client,
            string.Join(' ', arguments.Resource),
            _discoveryDocumentOptions.Value.Issuer,
            string.Join(' ', arguments.Scope),
            DateTime.UtcNow.AddSeconds(client.AccessTokenExpiration));

        await _identityContext.Set<ClientAccessToken>().AddAsync(accessToken);
        return accessToken.Reference;
    }
}