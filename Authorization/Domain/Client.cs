using Domain.Enums;

namespace Domain;

#nullable disable
public class Client
{
  public string Id { get; set; }
  public string Name { get; set; }
  public string Secret { get; set; }
  public string TosUri { get; set; }
  public string PolicyUri { get; set; }
  public TokenEndpointAuthMethod TokenEndpointAuthMethod { get; set; }
  public SubjectType SubjectType { get; set; }
  public ClientType ClientType { get; set; }
  public ClientProfile ClientProfile { get; set; }
  public ICollection<RedirectUri> RedirectUris { get; set; }
  public ICollection<GrantType> GrantTypes { get; set; }
  public ICollection<ConsentGrant> ConsentGrants { get; set; }
  public ICollection<Scope> Scopes { get; set; }
  public ICollection<Contact> Contacts { get; set; }
  public ICollection<ResponseType> ResponseTypes { get; set; }
  public ICollection<Session> Sessions { get; set; }
  public ICollection<AuthorizationCodeGrant> AuthorizationCodeGrants { get; set; }
  public ICollection<IdToken> IdTokens { get; set; }
  public ICollection<AccessToken> AccessTokens { get; set; }
  public ICollection<RefreshToken> RefreshTokens { get; set; }
  public ICollection<ClientRegistrationToken> ClientRegistrationTokens { get; set; }
}