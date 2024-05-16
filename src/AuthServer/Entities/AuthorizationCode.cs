using System.Linq.Expressions;
using AuthServer.Core;

namespace AuthServer.Entities;
public class AuthorizationCode : Entity<string>
{
    public AuthorizationCode(string value, DateTime issuedAt, AuthorizationGrant authorizationGrant)
    {
        Id = Guid.NewGuid().ToString();
        Value = string.IsNullOrWhiteSpace(value) ? throw new ArgumentNullException(nameof(value)) : value;
        IssuedAt = issuedAt;
        AuthorizationGrant = authorizationGrant ?? throw new ArgumentNullException(nameof(authorizationGrant));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected AuthorizationCode() { }
#pragma warning restore

    public string Value { get; private init; }
    public DateTime IssuedAt { get; private init; }
    public DateTime? RedeemedAt { get; private set; }
    public AuthorizationGrant AuthorizationGrant { get; private init; }

    public static Expression<Func<AuthorizationCode, bool>> IsValid(Client client) => a =>
      a.RedeemedAt == null && a.IssuedAt.AddSeconds(client.AuthorizationCodeExpiration) > DateTime.UtcNow;

    public void Redeem()
    {
        RedeemedAt ??= DateTime.UtcNow;
    }
}