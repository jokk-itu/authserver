using System.Text.Json.Serialization;

namespace Contracts.PostToken;
#nullable disable
public record PostTokenResponse
{
  [JsonPropertyName("access_token")]
  public string AccessToken { get; init; }

  [JsonPropertyName("refresh_token")]
  public string RefreshToken { get; init; }

  [JsonPropertyName("id_token")]
  public string IdToken { get; init; }

  [JsonPropertyName("token_type")]
  public const string TokenType = "Bearer";

  [JsonPropertyName("expires_in")]
  public int ExpiresIn { get; init; }
}
