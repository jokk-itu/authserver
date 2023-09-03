using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Context.ClientContext;
#nullable disable
public class ClientContext
{
  [JsonPropertyName(ParameterNames.RedirectUris)]
  public ICollection<string> RedirectUris { get; set; } = new List<string>();

  [JsonPropertyName(ParameterNames.PostLogoutRedirectUris)]
  public ICollection<string> PostLogoutRedirectUris { get; set; } = new List<string>();

  [JsonPropertyName(ParameterNames.ResponseTypes)]
  public ICollection<string> ResponseTypes { get; set; } = new List<string>();

  [JsonPropertyName(ParameterNames.GrantTypes)]
  public ICollection<string> GrantTypes { get; set; } = new List<string>();

  [JsonPropertyName(ParameterNames.ApplicationType)]
  public string ApplicationType { get; set; }

  [JsonPropertyName(ParameterNames.Contacts)]
  public ICollection<string> Contacts { get; set; } = new List<string>();

  [JsonPropertyName(ParameterNames.ClientName)]
  public string ClientName { get; set; }

  [JsonPropertyName(ParameterNames.PolicyUri)]
  public string PolicyUri { get; set; }

  [JsonPropertyName(ParameterNames.TosUri)]
  public string TosUri { get; set; }

  [JsonPropertyName(ParameterNames.SubjectType)]
  public string SubjectType { get; set; }

  [JsonPropertyName(ParameterNames.TokenEndpointAuthMethod)]
  public string TokenEndpointAuthMethod { get; set; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string Scope { get; set; }

  [JsonPropertyName(ParameterNames.DefaultMaxAge)]
  public string DefaultMaxAge { get; set; }

  [JsonPropertyName(ParameterNames.InitiateLoginUri)]
  public string InitiateLoginUri { get; set; }

  [JsonPropertyName(ParameterNames.LogoUri)]
  public string LogoUri { get; set; }

  [JsonPropertyName(ParameterNames.ClientUri)]
  public string ClientUri { get; set; }

  [JsonPropertyName(ParameterNames.BackChannelLogoutUri)]
  public string BackchannelLogoutUri { get; set; }
}