using Domain.Enums;

namespace Domain;

#nullable disable
public class Client
{
  public string Id { get; set; }

  public string Name { get; set; }

  public string Secret { get; set; }

  public ClientType ClientType { get; set; }

  public ClientProfile ClientProfile { get; set; }

  public ICollection<RedirectUri> RedirectUris { get; set; }

  public ICollection<Grant> Grants { get; set; }

  public ICollection<Scope> Scopes { get; set; }

  // Make TosUri
  // Make PolicyUri
  // Make ClientContact entity
  // Make SubjectType enum
  // Make TokenEndpointAuthMethod enum and constants
  // Make ResponseType entity and associate with client
}