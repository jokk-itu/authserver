using System.Linq.Expressions;
using System.Security.Cryptography;
using AuthServer.Core;
using AuthServer.Enums;

namespace AuthServer.Entities;
public abstract class Token : AggregateRoot<Guid>
{
    protected Token(TokenType tokenType, string audience, string issuer, string? scope, DateTime? expiresAt)
    {
        Id = Guid.NewGuid();
        Reference = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        IssuedAt = DateTime.UtcNow;
        TokenType = tokenType;
        NotBefore = DateTime.UtcNow;
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

    public virtual void Revoke()
    {
        RevokedAt ??= DateTime.UtcNow;
    }

    public static readonly Expression<Func<Token, bool>> IsActive =
        t => t.RevokedAt == null
             && t.NotBefore < DateTime.UtcNow
             && (t.ExpiresAt == null || t.ExpiresAt > DateTime.UtcNow);
}