using Domain;
using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.GetClient;

#nullable disable
public class GetClientResponse
{
  [JsonPropertyName(ParameterNames.ClientId)]
  public string ClientId { get; init; }

  [JsonPropertyName(ParameterNames.ClientName)]
  public string ClientName { get; init; }

  [JsonPropertyName(ParameterNames.ClientSecret)]
  public string ClientSecret { get; init; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string Scope { get; init; }

  [JsonPropertyName(ParameterNames.TosUri)]
  public string TosUri { get; init; }

  [JsonPropertyName(ParameterNames.PolicyUri)]
  public string PolicyUri { get; init; }

  [JsonPropertyName(ParameterNames.TokenEndpointAuthMethod)]
  public string TokenEndpointAuthMethod { get; init; }

  [JsonPropertyName(ParameterNames.SubjectType)]
  public string SubjectType { get; init; }

  [JsonPropertyName(ParameterNames.ApplicationType)]
  public string ApplicationType { get; init; }

  [JsonPropertyName(ParameterNames.RedirectUris)]
  public ICollection<string> RedirectUris { get; init; }

  [JsonPropertyName(ParameterNames.GrantTypes)]
  public ICollection<Domain.GrantType> GrantTypes { get; init; }

  [JsonPropertyName(ParameterNames.Contacts)]
  public ICollection<Contact> Contacts { get; init; }

  [JsonPropertyName(ParameterNames.ResponseTypes)]
  public ICollection<string> ResponseTypes { get; init; }
}