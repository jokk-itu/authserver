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

  [JsonPropertyName("end_session_endpoint")]
  public string EndSessionEndpoint { get; init; }

  [JsonPropertyName("jwks_uri")]
  public string JwksUri { get; init; }

  [JsonPropertyName("scopes_supported")]
  public IEnumerable<string> Scopes { get; init; }

  [JsonPropertyName("response_types_supported")]
  public IEnumerable<string> ResponseTypes { get; init; }

  [JsonPropertyName("grant_types_supported")]
  public IEnumerable<string> GrantTypes { get; init; }

  [JsonPropertyName("token_endpoint_auth_methods_supported")]
  public IEnumerable<string> TokenEndpointAuthMethods { get; init; }

  [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
  public IEnumerable<string> TokenEndpointAuthSigningAlgValues { get; init; }

  [JsonPropertyName("code_challenge_methods_supported")]
  public IEnumerable<string> CodeChallengeMethods { get; init; }

  [JsonPropertyName("subject_types_supported")]
  public IEnumerable<string> SubjectTypes { get; init; }

  [JsonPropertyName("id_token_signing_alg_values_supported")]
  public IEnumerable<string> IdTokenSigningAlgValues { get; init; }

  [JsonPropertyName("response_modes_supported")]
  public IEnumerable<string> ResponseModes { get; init; }

  [JsonPropertyName("authorization_response_iss_parameter_supported")]
  public bool AuthorizationResponseIssParameterSupported { get; init; }

  [JsonPropertyName("backchannel_logout_supported")]
  public bool BackChannelLogoutSupported { get; init; }
}