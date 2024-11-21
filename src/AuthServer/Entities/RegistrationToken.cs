using AuthServer.Enums;

namespace AuthServer.Entities;
public class RegistrationToken : ClientToken
{
    public RegistrationToken(Client client, string audience, string issuer, string? scope)
        : base(client, TokenType.RegistrationToken, audience, issuer, scope, null)
    {}

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected RegistrationToken() { }
#pragma warning restore
}