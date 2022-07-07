using Microsoft.AspNetCore.Identity;

namespace AuthorizationServer.Entities;

public class IdentityClientToken<TKey>
{
  public TKey ClientId { get; init; }

  public string Name { get; init; }

  [ProtectedPersonalData]
  public string Value { get; init; }
}