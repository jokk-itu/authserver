using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.PostRefreshToken;
public record PostRefreshTokenRequest
{
  [JsonPropertyName("grant_type")]
  public string GrantType { get; init; }

  [JsonPropertyName("refresh_token")]
  public string RefreshToken { get; init; }

  [JsonPropertyName("scope")]
  public string Scope { get; init; }
}
