using AuthServer.Enums;

namespace AuthServer.Entities;
public class ClientAccessToken : ClientToken
{
    public ClientAccessToken(Client client, string audience, string issuer, string? scope, DateTime? expiresAt)
        : base(client, TokenType.ClientAccessToken, audience, issuer, scope, expiresAt)
    {}

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private ClientAccessToken() { }
#pragma warning restore
}
