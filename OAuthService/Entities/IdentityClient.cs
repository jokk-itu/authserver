namespace OAuthService.Entities;

public class IdentityClient
{
    public string Id { get; set; }

    public string SecretHash { get; set; } //Hashed Secret by the PasswordHasher

    public string ConcurrencyStamp { get; } = Guid.NewGuid().ToString();
}