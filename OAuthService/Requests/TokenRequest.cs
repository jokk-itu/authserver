using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OAuthService.Requests;

public record TokenRequest
{
  [Required(AllowEmptyStrings = false)]
  [RegularExpression("authorization_code|refresh_token")]
  [JsonPropertyName("grant_type")]
  public string GrantType { get; init; }

  [JsonPropertyName("code")]
  public string? Code { get; init; }

  [Required(AllowEmptyStrings = false)]
  [JsonPropertyName("redirect_uri")]
  public string RedirectUri { get; init; }

  [JsonPropertyName("refresh_token")]
  public string? RefreshToken { get; init; }

  [JsonPropertyName("scope")]
  //TODO why is this here
  public string? Scope { get; init; }

  [JsonPropertyName("code_verifier")]
  public string? CodeVerifier { get; init; }
}