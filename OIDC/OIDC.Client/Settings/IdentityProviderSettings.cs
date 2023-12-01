using System.Collections.Generic;

namespace OIDC.Client.Settings;

public class IdentityProviderSettings
{
  public string ClientId { get; set; }
  public string ClientSecret { get; set; }
  public string ClientName { get; set; }
  public string Authority { get; set; }
  public int? MaxAge { get; set; }
  public IEnumerable<string> Scope { get; set; }
  public string ResponseMode { get; set; }
  public IEnumerable<string> GrantTypes { get; set; }
  public string ClientAuthenticationMethod { get; set; }
  public string Resource { get; set; }
}