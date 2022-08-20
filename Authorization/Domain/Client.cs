using Domain;

namespace Domain;

#nullable disable
public class Client
{
  public string Id { get; set; }

  public string SecretHash { get; set; }

  public ClientType ClientType { get; set; }

  public ClientProfile ClientProfile { get; set; }

  public ICollection<RedirectUri> RedirectUris { get; set; }

  public ICollection<Grant> Grants { get; set; }

  public ICollection<Scope> Scopes { get; set; }
}