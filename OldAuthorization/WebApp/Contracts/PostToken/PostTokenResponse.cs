using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.PostToken;
public record PostTokenResponse
{
  [JsonPropertyName(ParameterNames.AccessToken)]
  public string AccessToken { get; init; } = null!;

  [JsonPropertyName(ParameterNames.RefreshToken)]
  public string? RefreshToken { get; init; }

  [JsonPropertyName(ParameterNames.IdToken)]
  public string? IdToken { get; init; }

  [JsonPropertyName(ParameterNames.TokenType)]
  public string TokenType { get; init; } = "Bearer";

  [JsonPropertyName(ParameterNames.ExpiresIn)]
  public long ExpiresIn { get; init; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string Scope { get; init; } = null!;
}