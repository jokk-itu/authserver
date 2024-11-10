using System.Text.Json.Serialization;
using AuthServer.Constants;

namespace AuthServer.Options;

public class DiscoveryDocument
{
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = null!;

    [JsonPropertyName("service_documentation")]
    public string? ServiceDocumentation { get; set; }

    [JsonPropertyName("op_policy_uri")]
    public string? OpPolicyUri { get; set; }

    [JsonPropertyName("op_tos_uri")]
    public string? OpTosUri { get; set; }

    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint => $"{Issuer}/connect/authorize";

    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint => $"{Issuer}/connect/token";

    [JsonPropertyName("userinfo_endpoint")]
    public string UserinfoEndpoint => $"{Issuer}/connect/userinfo";

    [JsonPropertyName("jwks_uri")]
    public string JwksUri => $"{Issuer}/.well-known/jwks";

    [JsonPropertyName("registration_endpoint")]
    public string RegistrationEndpoint => $"{Issuer}/connect/register";

    [JsonPropertyName("end_session_endpoint")]
    public string EndSessionEndpoint => $"{Issuer}/connect/end-session";

    [JsonPropertyName("introspection_endpoint")]
    public string IntrospectionEndpoint => $"{Issuer}/connect/introspection";

    [JsonPropertyName("revocation_endpoint")]
    public string RevocationEndpoint => $"{Issuer}/connect/revocation";

    [JsonPropertyName("pushed_authorization_request_endpoint")]
    public string PushedAuthorizationRequestEndpoint => $"{Issuer}/connect/par";

    [JsonPropertyName("claims_support")]
    public ICollection<string> ClaimsSupported { get; set; } = [];

    [JsonPropertyName("claim_types_supported")]
    public ICollection<string> ClaimTypesSupported => ["normal"];

    [JsonPropertyName("prompt_values_supported")]
    public ICollection<string> PromptValuesSupported => PromptConstants.Prompts;

    [JsonPropertyName("display_values_supported")]
    public ICollection<string> DisplayValuesSupported => DisplayConstants.DisplayValues;

    [JsonPropertyName("subject_types_supported")]
    public ICollection<string> SubjectTypesSupported => SubjectTypeConstants.SubjectTypes;

    [JsonPropertyName("grant_types_supported")]
    public ICollection<string> GrantTypesSupported => GrantTypeConstants.GrantTypes;

    [JsonPropertyName("acr_values_supported")]
    public ICollection<string> AcrValuesSupported { get; set; } = [];

    [JsonPropertyName("challenge_methods_supported")]
    public ICollection<string> ChallengeMethodsSupported => CodeChallengeMethodConstants.CodeChallengeMethods;

    [JsonPropertyName("scopes_supported")]
    public ICollection<string> ScopesSupported { get; set; } = [];

    [JsonPropertyName("response_types_supported")]
    public ICollection<string> ResponseTypesSupported => ResponseTypeConstants.ResponseTypes;

    [JsonPropertyName("response_modes_supported")]
    public ICollection<string> ResponseModesSupported => ResponseModeConstants.ResponseModes;

    [JsonPropertyName("introspection_endpoint_auth_methods_supported")]
    public ICollection<string> IntrospectionEndpointAuthMethodsSupported => IntrospectionEndpointAuthMethodConstants.AuthMethods;

    [JsonPropertyName("revocation_endpoint_auth_methods_supported")]
    public ICollection<string> RevocationEndpointAuthMethodsSupported => RevocationEndpointAuthMethodConstants.AuthMethods;

    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public ICollection<string> TokenEndpointAuthMethodsSupported => TokenEndpointAuthMethodConstants.AuthMethods;

    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public ICollection<string> IdTokenSigningAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("id_token_encryption_alg_values_supported")]
    public ICollection<string> IdTokenEncryptionAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("id_token_encryption_enc_values_supported")]
    public ICollection<string> IdTokenEncryptionEncValuesSupported { get; set; } = [];

    [JsonPropertyName("userinfo_signing_alg_values_supported")]
    public ICollection<string> UserinfoSigningAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("userinfo_encryption_alg_values_supported")]
    public ICollection<string> UserinfoEncryptionAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("userinfo_encryption_enc_values_supported")]
    public ICollection<string> UserinfoEncryptionEncValuesSupported { get; set; } = [];

    [JsonPropertyName("request_object_signing_alg_values_supported")]
    public ICollection<string> RequestObjectSigningAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("request_object_encryption_alg_values_supported")]
    public ICollection<string> RequestObjectEncryptionAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("request_object_encryption_enc_values_supported")]
    public ICollection<string> RequestObjectEncryptionEncValuesSupported { get; set; } = [];

    [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
    public ICollection<string> TokenEndpointAuthSigningAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("introspection_endpoint_auth_signing_alg_values_supported")]
    public ICollection<string> IntrospectionEndpointAuthSigningAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("revocation_endpoint_auth_signing_alg_values_supported")]
    public ICollection<string> RevocationEndpointAuthSigningAlgValuesSupported { get; set; } = [];

    [JsonPropertyName("authorization_response_iss_parameter_supported")]
    public bool AuthorizationResponseIssParameterSupported => true;

    [JsonPropertyName("backchannel_logout_supported")]
    public bool BackchannelLogoutSupported => true;

    [JsonPropertyName("require_request_uri_registration")]
    public bool RequireRequestUriRegistration => true;

    [JsonPropertyName("claims_parameter_supported")]
    public bool ClaimsParameterSupported => false;

    [JsonPropertyName("request_parameter_supported")]
    public bool RequestParameterSupported => true;

    [JsonPropertyName("request_uri_parameter_supported")]
    public bool RequestUriParameterSupported => true;

    [JsonPropertyName("require_signed_request_object")]
    public bool RequireSignedRequestObject { get; set; }

    [JsonPropertyName("require_pushed_authorization_requests")]
    public bool RequirePushedAuthorizationRequests { get; set; }
}