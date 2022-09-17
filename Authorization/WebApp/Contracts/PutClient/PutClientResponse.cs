using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.PutClient;

#nullable disable
public class PutClientResponse
{
  [JsonPropertyName(ParameterNames.ClientId)]
  public string ClientId { get; init; }

  [JsonPropertyName(ParameterNames.ClientSecret)]
  public string ClientSecret { get; init; }

  [JsonPropertyName(ParameterNames.ClientSecretExpiresAt)]
  public int ClientSecretExpiresAt { get; init; }

  [JsonPropertyName(ParameterNames.RegistrationAccessToken)]
  public string RegistrationAccessToken { get; init; }

  [JsonPropertyName(ParameterNames.RegistrationClientUri)]
  public string RegistrationClientUri { get; init; }
}
