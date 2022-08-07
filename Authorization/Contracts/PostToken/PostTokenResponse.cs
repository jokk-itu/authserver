using System.Text.Json.Serialization;

namespace Contracts.PostToken;
public class PostTokenResponse
{
  [JsonPropertyName("access_token")]
  public string AccessToken { get; set; }

  [JsonPropertyName("refresh_token")]
  public string RefreshToken { get; set; }

  [JsonPropertyName("id_token")]
  public string IdToken { get; set; }

  [JsonPropertyName("token_type")]
  public string TokenType => "Bearer";

  [JsonPropertyName("expires_in")]
  public int ExpiresIn { get; set; }
}
