using System.Text.Json.Serialization;

namespace WebApp.Contracts.GetDiscoveryDocument;

#nullable disable
public record GetDiscoveryDocumentResponse
{
  [JsonPropertyName("issuer")]
  public string Issuer { get; init; }

  [JsonPropertyName("authorization_endpoint")]
  public string AuthorizationEndpoint { get; init; }

  [JsonPropertyName("token_endpoint")]
  public string TokenEndpoint { get; init; }

  [JsonPropertyName("userinfo_endpoint")]
  public string UserInfoEndpoint { get; init; }

  [JsonPropertyName("jwks_uri")]
  public string JwksUri { get; init; }

  [JsonPropertyName("scopes_supported")]
  public IEnumerable<string> Scopes { get; init; }

  [JsonPropertyName("response_types_supported")]
  public IEnumerable<string> ResponseTypes { get; } = new[] { "code" };

  [JsonPropertyName("grant_types_supported")]
  public IEnumerable<string> GrantTypes { get; } = new[] { "authorization_code", "refresh_token" };

  [JsonPropertyName("token_endpoint_auth_methods_supported")]
  public IEnumerable<string> TokenEndpointAuthMethods { get; } = new[] { "client_secret_basic" };

  [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
  public IEnumerable<string> TokenEndpointAuthSigningAlgValues { get; } = new[] { "RS256" };

  [JsonPropertyName("code_challenge_methods_supported")]
  public IEnumerable<string> CodeChallengeMethods { get; } = new[] { "S256" };

  [JsonPropertyName("subject_types_supported")]
  public IEnumerable<string> SubjectTypes { get; } = new[] { "public" };

  [JsonPropertyName("id_token_signing_alg_values_supported")]
  public IEnumerable<string> IdTokenSigningAlgValues { get; } = new[] { "RS256" };

  [JsonPropertyName("response_modes_supported")]
  public IEnumerable<string> ResponseModes { get; } = new[] { "query" };
}