namespace OAuthService.Entities;

public class IdentityClient
{
    public string Id { get; set; }

    public string SecretHash { get; set; }

    public string ClientType { get; set; }

    public string ClientProfile { get; set; }

    public string ConcurrencyStamp { get; } = Guid.NewGuid().ToString();
}