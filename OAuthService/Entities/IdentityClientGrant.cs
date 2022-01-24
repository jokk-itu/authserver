namespace OAuthService.Entities;

public class IdentityClientGrant<TKey>
{
  public string Name { get; init; }

  public TKey ClientId { get; init; }
}