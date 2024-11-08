using AuthServer.Core;

namespace AuthServer.Entities;
public class AuthorizationCode : Entity<string>
{
    public AuthorizationCode(AuthorizationGrant authorizationGrant, int expirationSeconds)
    {
        Id = Guid.NewGuid().ToString();
        IssuedAt = DateTime.UtcNow;
        AuthorizationGrant = authorizationGrant ?? throw new ArgumentNullException(nameof(authorizationGrant));
        ExpiresAt = IssuedAt.AddSeconds(expirationSeconds);
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private AuthorizationCode() { }
#pragma warning restore

    public string Value { get; private set; } = null!;
    public DateTime IssuedAt { get; private init; }
    public DateTime ExpiresAt { get; private init; }
    public DateTime? RedeemedAt { get; private set; }
    public AuthorizationGrant AuthorizationGrant { get; private init; }

    public void Redeem()
    {
        RedeemedAt ??= DateTime.UtcNow;
    }

    public void SetValue(string value)
    {
        Value = value;
    }
}