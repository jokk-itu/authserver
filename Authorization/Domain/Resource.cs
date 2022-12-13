namespace Domain;

#nullable disable
public class Resource
{
  public string Id { get; set; }
  public string Name { get; set; }
  public string Secret { get; set; }
  public ICollection<Scope> Scopes { get; set; } = new List<Scope>();
  public ICollection<ResourceRegistrationToken> ResourceRegistrationTokens { get; set; } = new List<ResourceRegistrationToken>();
}