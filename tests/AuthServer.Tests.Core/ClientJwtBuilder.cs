using AuthServer.Core.Discovery;
using AuthServer.TokenDecoders;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Tests.Core;
public class ClientJwtBuilder
{
    private readonly DiscoveryDocument _discoveryDocument;

    public ClientJwtBuilder(DiscoveryDocument discoveryDocument)
    {
        _discoveryDocument = discoveryDocument;
    }

    public string GetPrivateKeyJwt(string clientId, string privateJwks, ClientTokenAudience audience)
    {
        var jwks = JsonWebKeySet.Create(privateJwks);
        var jsonWebKey = jwks.Keys.First(k => k.Use == JsonWebKeyUseNames.Sig);
        var signingCredentials = new SigningCredentials(jsonWebKey, jsonWebKey.Alg);
        var now = DateTime.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = clientId,
            NotBefore = now,
            Expires = now.AddSeconds(30),
            IssuedAt = now,
            SigningCredentials = signingCredentials,
            Audience = MapToAudience(audience)
        });
    }

    private string MapToAudience(ClientTokenAudience audience)
        => audience switch
        {
            ClientTokenAudience.AuthorizeEndpoint => _discoveryDocument.AuthorizationEndpoint,
            ClientTokenAudience.TokenEndpoint => _discoveryDocument.TokenEndpoint,
            ClientTokenAudience.IntrospectionEndpoint => _discoveryDocument.IntrospectionEndpoint,
            ClientTokenAudience.RevocationEndpoint => _discoveryDocument.RevocationEndpoint,
            _ => throw new ArgumentException("unknown value", nameof(audience))
        };
}