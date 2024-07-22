using System.Text.Json;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Discovery;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;

internal class ClientAccessTokenBuilder : ITokenBuilder<ClientAccessTokenArguments>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    public ClientAccessTokenBuilder(
        AuthorizationDbContext identityContext,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _identityContext = identityContext;
        _jwksDocumentOptions = jwksDocumentOptions;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public async Task<string> BuildToken(ClientAccessTokenArguments arguments, CancellationToken cancellationToken)
    {
        var client = await _identityContext
            .Set<Client>()
            .Where(x => x.Id == arguments.ClientId)
            .SingleAsync(cancellationToken);

        if (client.RequireReferenceToken)
        {
            return await BuildReferenceToken(arguments, client);
        }

        return BuildStructuredToken(arguments, client);
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