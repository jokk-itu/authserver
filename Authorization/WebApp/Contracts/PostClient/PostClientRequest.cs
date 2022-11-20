using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.PostClient;

#nullable disable
public class PostClientRequest
{
  [JsonPropertyName(ParameterNames.RedirectUris)]
  public ICollection<string> RedirectUris { get; init; }

  [JsonPropertyName(ParameterNames.ResponseTypes)]
  public ICollection<string> ResponseTypes { get; init; }

  [JsonPropertyName(ParameterNames.GrantTypes)]
  public ICollection<string> GrantTypes { get; init; }

  [JsonPropertyName(ParameterNames.ApplicationType)]
  public string ApplicationType { get; init; }

  [JsonPropertyName(ParameterNames.Contacts)]
  public ICollection<string> Contacts { get; init; }

  [JsonPropertyName(ParameterNames.ClientName)]
  public string ClientName { get; init; }

  [JsonPropertyName(ParameterNames.PolicyUri)]
  public string PolicyUri { get; init; }

  [JsonPropertyName(ParameterNames.TosUri)]
  public string TosUri { get; init; }

  [JsonPropertyName(ParameterNames.SubjectType)]
  public string SubjectType { get; init; }

  [JsonPropertyName(ParameterNames.TokenEndpointAuthMethod)]
  public string TokenEndpointAuthMethod { get; init; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string Scope { get; init; }
}