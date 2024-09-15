using System.Linq.Expressions;
using AuthServer.Core;

namespace AuthServer.Entities;
public class AuthorizationCode : Entity<string>
{
    public AuthorizationCode(AuthorizationGrant authorizationGrant)
    {
        Id = Guid.NewGuid().ToString();
        IssuedAt = DateTime.UtcNow;
        AuthorizationGrant = authorizationGrant ?? throw new ArgumentNullException(nameof(authorizationGrant));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private AuthorizationCode() { }
#pragma warning restore

    public string Value { get; private set; } = null!;
    public DateTime IssuedAt { get; private init; }
    public DateTime? RedeemedAt { get; private set; }
    public AuthorizationGrant AuthorizationGrant { get; private init; }

    public static Expression<Func<AuthorizationCode, bool>> IsValid(Client client) => a =>
      a.RedeemedAt == null && a.IssuedAt.AddSeconds(client.AuthorizationCodeExpiration!.Value) > DateTime.UtcNow;

    public void Redeem()
    {
        RedeemedAt ??= DateTime.UtcNow;
    }

    public void SetValue(string value)
    {
        Value = value;
    }
}