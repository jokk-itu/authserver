using AuthServer.Enums;

namespace AuthServer.Entities;
public abstract class GrantToken : Token
{
    protected GrantToken(AuthorizationGrant authorizationGrant, TokenType tokenType, string audience, string issuer, string? scope, DateTime? expiresAt)
        : base(tokenType, audience, issuer, scope, expiresAt)
    {
        AuthorizationGrant = authorizationGrant ?? throw new ArgumentNullException(nameof(authorizationGrant));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected GrantToken() { }
#pragma warning restore

    public AuthorizationGrant AuthorizationGrant { get; private init; }
}