namespace WebApp.Context.ClientContext;
#nullable disable
public class ClientContext
{
  public ICollection<string> RedirectUris { get; set; } = new List<string>();
  public ICollection<string> PostLogoutRedirectUris { get; set; } = new List<string>();
  public ICollection<string> ResponseTypes { get; set; } = new List<string>();
  public ICollection<string> GrantTypes { get; set; } = new List<string>();
  public string ApplicationType { get; set; }
  public ICollection<string> Contacts { get; set; } = new List<string>();
  public string ClientName { get; set; }
  public string PolicyUri { get; set; }
  public string TosUri { get; set; }
  public string SubjectType { get; set; }
  public string TokenEndpointAuthMethod { get; set; }
  public string Scope { get; set; }
  public string DefaultMaxAge { get; set; }
  public string InitiateLoginUri { get; set; }
  public string LogoUri { get; set; }
  public string ClientUri { get; set; }
  public string BackChannelLogoutUri { get; set; }
}