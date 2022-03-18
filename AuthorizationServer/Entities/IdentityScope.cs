namespace AuthorizationServer.Entities;

public class IdentityScope
{
  public string Id { get; init; }

  public string ConcurrencyStamp { get; } = Guid.NewGuid().ToString();
}