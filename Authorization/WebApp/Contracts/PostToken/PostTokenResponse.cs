using System.Text.Json.Serialization;
using WebApp.Constants;

namespace Contracts.PostToken;
#nullable disable
public record PostTokenResponse
{
  [JsonPropertyName(ParameterNames.AccessToken)]
  public string AccessToken { get; init; }

  [JsonPropertyName(ParameterNames.RefreshToken)]
  public string RefreshToken { get; init; }

  [JsonPropertyName(ParameterNames.IdToken)]
  public string IdToken { get; init; }

  [JsonPropertyName(ParameterNames.TokenType)]
  public const string TokenType = "Bearer";

  [JsonPropertyName(ParameterNames.ExpiresIn)]
  public int ExpiresIn { get; init; }
}
