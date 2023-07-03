using System.Collections.Generic;

namespace OIDC.Client.Settings;
public class IdentityProviderSettings
{
    public string Authority { get; set; }
    public int? MaxAge { get; set; }
    public IEnumerable<string> Scope { get; set; }
    public string ResponseMode { get; set; }
    public IEnumerable<string> Prompt { get; set; }
    public string ClientName { get; set; }
    public string ClientUri { get; set; }
    public IEnumerable<string> GrantTypes { get; set; }
    public string TokenEndpointAuthMethod { get; set; }
}