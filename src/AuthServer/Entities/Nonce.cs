using AuthServer.Core;

namespace AuthServer.Entities;
public class Nonce : Entity<string>
{
    public Nonce(string value, string hashedValue, AuthorizationGrant authorizationGrant)
    {
        Id = Guid.NewGuid().ToString();
        Value = string.IsNullOrWhiteSpace(value) ? throw new ArgumentNullException(nameof(value)) : value;
        HashedValue = string.IsNullOrWhiteSpace(hashedValue) ? throw new ArgumentNullException(nameof(hashedValue)) : hashedValue;
        IssuedAt = DateTime.UtcNow;
        AuthorizationGrant = authorizationGrant ?? throw new ArgumentNullException(nameof(authorizationGrant));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private Nonce() { }
#pragma warning restore

    public string Value { get; private init; }
    public string HashedValue { get; private init; }
    public DateTime IssuedAt { get; private init; }
    public AuthorizationGrant AuthorizationGrant { get; private init; }
}