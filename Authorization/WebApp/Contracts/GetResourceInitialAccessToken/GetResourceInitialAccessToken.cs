using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.GetResourceInitialAccessToken;

#nullable disable
public class GetResourceInitialAccessToken
{
  [JsonPropertyName(ParameterNames.AccessToken)]
  public string AccessToken { get; init; }

  [JsonPropertyName(ParameterNames.ExpiresIn)]
  public int ExpiresIn { get; init; }
}