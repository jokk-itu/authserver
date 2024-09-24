using AuthServer.Enums;

namespace AuthServer.Entities;
public class RefreshToken : GrantToken
{
    public RefreshToken(AuthorizationGrant authorizationGrant, string audience, string issuer, string? scope, DateTime? expiresAt)
        : base(authorizationGrant, TokenType.RefreshToken, audience, issuer, scope, expiresAt)
    {}

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected RefreshToken() { }
#pragma warning restore
}