using AuthServer.Enums;

namespace AuthServer.Entities;
public class GrantAccessToken : GrantToken
{
    public GrantAccessToken(AuthorizationGrant authorizationGrant, string audience, string issuer, string? scope, DateTime? expiresAt)
        : base(authorizationGrant, TokenType.GrantAccessToken, audience, issuer, scope, expiresAt)
    {}

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private GrantAccessToken() { }
#pragma warning restore
}