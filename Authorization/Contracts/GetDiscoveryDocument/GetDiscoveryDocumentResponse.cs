using System.Text.Json.Serialization;

namespace Contracts.GetDiscovery;
public class GetDiscoveryDocumentResponse
{
  [JsonPropertyName("issuer")]
  public string Issuer { get; set; }

  [JsonPropertyName("authorization_endpoint")]
  public string AuthorizationEndpoint { get; set; }

  [JsonPropertyName("token_endpoint")]
  public string TokenEndpoint { get; set; }

  [JsonPropertyName("jwks_uri")]
  public string JwksUri { get; set; }

  [JsonPropertyName("scopes_supported")]
  public IEnumerable<string> Scopes { get; set; }

  [JsonPropertyName("response_types_supported")]
  public IEnumerable<string> ResponseTypes => new string[] { "code" };

  [JsonPropertyName("grant_types_supported")]
  public IEnumerable<string> GrantTypes => new string[] { "authorization_code", "refresh_token" };

  [JsonPropertyName("token_endpoint_auth_methods_supported")]
  public IEnumerable<string> TokenEndpointAuthMethods => new string[] { "client_secret_basic" };

  [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
  public IEnumerable<string> TokenEndpointAuthSigningAlgValues => new string[] { "RS256" };

  [JsonPropertyName("code_challenge_methods_supported")]
  public IEnumerable<string> CodeChallengeMethods => new string[] { "S256" };

  [JsonPropertyName("subject_types_supported")]
  public IEnumerable<string> SubjectTypes => new string[] { "public" };

  [JsonPropertyName("id_token_signing_alg_values_supported")]
  public IEnumerable<string> IdTokenSigningAlgValues => new string[] { "RS256" };

  [JsonPropertyName("response_modes_supported")]
  public IEnumerable<string> ResponseModes => new string[] { "query" };
}