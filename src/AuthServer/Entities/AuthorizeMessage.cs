using System.Linq.Expressions;
using AuthServer.Core;
using AuthServer.Helpers;

namespace AuthServer.Entities;
public class AuthorizeMessage : Entity<Guid>
{
    public AuthorizeMessage(string value, DateTime expiresAt, Client client)
    {
        Id = Guid.NewGuid();
        Value = string.IsNullOrWhiteSpace(value) ? throw new ArgumentNullException(nameof(value)) : value;
        ExpiresAt = expiresAt;
        Reference = CryptographyHelper.GetRandomString(16);
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private AuthorizeMessage() { }
#pragma warning restore

    public string Reference { get; private init; }
    public string Value { get; private init; }
    public DateTime? RedeemedAt { get; private set; }
    public DateTime ExpiresAt { get; private init; }
    public Client Client { get; private init; }

    public static readonly Expression<Func<AuthorizeMessage, bool>> IsActive =
        x => x.RedeemedAt == null && x.ExpiresAt > DateTime.UtcNow;

    public void Redeem()
    {
        RedeemedAt ??= DateTime.UtcNow;
    }
}