using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.GetClientInitialAccessToken;

#nullable disable
public class GetClientInitialAccessTokenResponse
{
  [JsonPropertyName(ParameterNames.AccessToken)]
  public string AccessToken { get; init; }

  [JsonPropertyName(ParameterNames.ExpiresIn)]
  public int ExpiresIn { get; init; }
}
