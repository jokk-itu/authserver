namespace AuthorizationServer.Entities;

public class IdentityResource
{
  public string Id { get; init; }

  public string ConcurrencyStamp { get; } = Guid.NewGuid().ToString();
}