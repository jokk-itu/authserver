using System.Linq.Expressions;
using AuthServer.Core;
using AuthServer.Enums;
using AuthServer.Helpers;

namespace AuthServer.Entities;
public abstract class Token : Entity<Guid>
{
    protected Token(TokenType tokenType, string audience, string issuer, string? scope, DateTime? expiresAt)
    {
        var now = DateTime.UtcNow;
        Id = Guid.NewGuid();
        Reference = CryptographyHelper.GetRandomString(16);
        IssuedAt = now;
        TokenType = tokenType;
        NotBefore = now;
        Audience = string.IsNullOrWhiteSpace(audience) ? throw new ArgumentNullException(nameof(audience)) : audience;
        Issuer = string.IsNullOrWhiteSpace(issuer) ? throw new ArgumentNullException(nameof(issuer)) : issuer;
        Scope = scope;
        ExpiresAt = expiresAt;
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected Token() { }
#pragma warning restore

    public string Reference { get; private init; }
    public string? Scope { get; private init; }
    public TokenType TokenType { get; private init; }
    public DateTime? ExpiresAt { get; private init; }
    public DateTime IssuedAt { get; private init; }
    public DateTime NotBefore { get; private init; }
    public string Audience { get; private init; }
    public string Issuer { get; private init; }
    public DateTime? RevokedAt { get; set; }

    public void Revoke()
    {
        RevokedAt ??= DateTime.UtcNow;
    }

    public static readonly Expression<Func<Token, bool>> IsActive =
        t => t.RevokedAt == null
             && t.NotBefore < DateTime.UtcNow
             && (t.ExpiresAt == null || t.ExpiresAt > DateTime.UtcNow);
}