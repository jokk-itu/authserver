using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OIDC.Client.Registration;
internal class RegistrationRequest
{
    [JsonPropertyName("redirect_uris")]
    public IEnumerable<string> RedirectUris { get; init; }

    [JsonPropertyName("post_logout_redirect_uris")]
    public IEnumerable<string> PostLogoutRedirectUris { get; init; } = new List<string>();

    [JsonPropertyName("response_types")]
    public IEnumerable<string> ResponseTypes { get; init; }

    [JsonPropertyName("grant_types")]
    public IEnumerable<string> GrantTypes { get; init; }

    [JsonPropertyName("application_type")]
    public string ApplicationType { get; init; }

    [JsonPropertyName("contacts")]
    public IEnumerable<string> Contacts { get; init; } = new List<string>();

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

    [JsonPropertyName("default_max_age")]
    public string DefaultMaxAge { get; init; }

    [JsonPropertyName("initiate_login_uri")]
    public string InitiateLoginUri { get; init; }

    [JsonPropertyName("logo_uri")]
    public string LogoUri { get; init; }

    [JsonPropertyName("client_uri")]
    public string ClientUri { get; init; }

    [JsonPropertyName("backchannel_logout_uri")]
    public string BackChannelLogoutUri { get; init; }
}