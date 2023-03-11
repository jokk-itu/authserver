using Domain.Enums;

namespace Domain;
public class Client
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public string Name { get; set; } = null!;
  public string? Secret { get; set; }
  public string? TosUri { get; set; }
  public string? PolicyUri { get; set; }
  public string? ClientUri { get; set; }
  public string? LogoUri { get; set; }
  public string? InitiateLoginUri { get; set; }
  public string? BackChannelLogoutUri { get; set; }
  public long? DefaultMaxAge { get; set; }
  public ApplicationType ApplicationType { get; set; }
  public TokenEndpointAuthMethod TokenEndpointAuthMethod { get; set; }
  public SubjectType SubjectType { get; set; }
  public ICollection<RedirectUri> RedirectUris { get; set; } = new List<RedirectUri>();
  public ICollection<GrantType> GrantTypes { get; set; } = new List<GrantType>();
  public ICollection<ConsentGrant> ConsentGrants { get; set; } = new List<ConsentGrant>();
  public ICollection<Scope> Scopes { get; set; } = new List<Scope>();
  public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
  public ICollection<ResponseType> ResponseTypes { get; set; } = new List<ResponseType>();
  public ICollection<AuthorizationCodeGrant> AuthorizationCodeGrants { get; set; } = new List<AuthorizationCodeGrant>();
}