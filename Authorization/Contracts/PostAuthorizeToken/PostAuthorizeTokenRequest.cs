using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.PostAuthorizeToken;

public record PostAuthorizeTokenRequest
{
  [JsonPropertyName("grant_type")]
  public string GrantType { get; init; }

  [JsonPropertyName("code")]
  public string Code { get; init; }

  [JsonPropertyName("redirect_uri")]
  public string RedirectUri { get; init; }

  [JsonPropertyName("scope")]
  public string Scope { get; init; }

  [JsonPropertyName("code_verifier")]
  public string CodeVerifier { get; init; }
}