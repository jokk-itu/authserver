using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OIDC.Client.Registration;
public class RegistrationResponse
{
    [JsonPropertyName("redirect_uris")]
    public ICollection<string> RedirectUris { get; init; }

    [JsonPropertyName("response_types")]
    public ICollection<string> ResponseTypes { get; init; }

    [JsonPropertyName("grant_types")]
    public ICollection<string> GrantTypes { get; init; }

    [JsonPropertyName("application_type")]
    public string ApplicationType { get; init; }

    [JsonPropertyName("contacts")]
    public ICollection<string> Contacts { get; init; }

    [JsonPropertyName("client_name")]
    public string ClientName { get; init; }

    [JsonPropertyName("policy_uri")]
    public string PolicyUri { get; init; }

    [JsonPropertyName("tos_uri")]
    public string TosUri { get; init; }

    [JsonPropertyName("subject_type")]
    public string SubjectType { get; init; }

    [JsonPropertyName("token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; init; }

    [JsonPropertyName("scope")]
    public string Scope { get; init; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; init; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; init; }

    [JsonPropertyName("client_secret_expires_at")]
    public int ClientSecretExpiresAt { get; init; }

    [JsonPropertyName("registration_access_token")]
    public string RegistrationAccessToken { get; init; }

    [JsonPropertyName("registration_client_uri")]
    public string RegistrationClientUri { get; init; }

    [JsonPropertyName("default_max_age")]
    public string DefaultMaxAge { get; init; }

    [JsonPropertyName("initiate_login_uri")]
    public string InitiateLoginUri { get; init; }

    [JsonPropertyName("logo_uri")]
    public string LogoUri { get; init; }

    [JsonPropertyName("client_uri")]
    public string ClientUri { get; init; }

    [JsonPropertyName("post_logout_redirect_uris")]
    public ICollection<string> PostLogoutRedirectUris { get; init; }

    [JsonPropertyName("backchannel_logout_uri")]
    public string BackChannelLogoutUri { get; init; }
}
