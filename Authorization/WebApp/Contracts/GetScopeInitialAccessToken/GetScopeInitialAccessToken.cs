using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.GetScopeInitialAccessToken;

#nullable disable
public class GetScopeInitialAccessToken
{
  [JsonPropertyName(ParameterNames.AccessToken)]
  public string AccessToken { get; init; }

  [JsonPropertyName(ParameterNames.ExpiresIn)]
  public int ExpiresIn { get; init; }
}
