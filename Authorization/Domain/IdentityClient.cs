namespace AuthorizationServer.Entities;

public class IdentityClient
{
  public string Id { get; init; }

  public string SecretHash { get; init; }

  public string ClientType { get; init; }

  public string ClientProfile { get; init; }

  public string ConcurrencyStamp { get; } = Guid.NewGuid().ToString();
}