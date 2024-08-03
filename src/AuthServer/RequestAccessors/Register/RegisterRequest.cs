namespace AuthServer.RequestAccessors.Register;

public class RegisterRequest
{
    public HttpMethod Method { get; set; } = null!;
    public string ClientId { get; init; } = null!;
    public string RegistrationAccessToken { get; init; } = null!;
    public string ClientName { get; init; } = null!;
    public string ApplicationType { get; init; } = null!;
    public string SubjectType { get; init; } = null!;
    public string DefaultMaxAge { get; init; } = null!;
    public string TokenEndpointAuthMethod { get; init; } = null!;
    public string TokenEndpointAuthSigningAlg { get; init; } = null!;
    public string Jwks { get; init; } = null!;
    public string JwksUri { get; init; } = null!;
    public string ClientUri { get; init; } = null!;
    public string PolicyUri { get; init; } = null!;
    public string TosUri { get; init; } = null!;
    public string InitiateLoginUri { get; init; } = null!;
    public string LogoUri { get; init; } = null!;
    public string BackchannelLogoutUri { get; init; } = null!;
    public string RequireSignedRequestObject { get; init; } = null!;
    public string RequireReferenceToken { get; init; } = null!;
    public string RequestObjectEncryptionEnc { get; init; } = null!;
    public string RequestObjectEncryptionAlg { get; init; } = null!;
    public string RequestObjectSigningAlg { get; init; } = null!;
    public string UserinfoEncryptedResponseEnc { get; init; } = null!;
    public string UserinfoEncryptedResponseAlg { get; init; } = null!;
    public string UserinfoSignedResponseAlg { get; init; } = null!;
    public string IdTokenEncryptedResponseEnc { get; init; } = null!;
    public string IdTokenEncryptedResponseAlg { get; init; } = null!;
    public string IdTokenSignedResponseAlg { get; init; } = null!;
    public string AuthorizationCodeExpiration { get; init; } = null!;
    public string AccessTokenExpiration { get; init; } = null!;
    public string RefreshTokenExpiration { get; init; } = null!;
    public string ClientSecretExpiration { get; init; } = null!;
    public string JwksExpiration { get; init; } = null!;
    public IReadOnlyCollection<string> DefaultAcrValues { get; init; } = null!;
    public IReadOnlyCollection<string> Scope { get; init; } = null!;
    public IReadOnlyCollection<string> RedirectUris { get; init; } = null!;
    public IReadOnlyCollection<string> PostLogoutRedirectUris { get; init; } = null!;
    public IReadOnlyCollection<string> RequestUris { get; init; } = null!;
    public IReadOnlyCollection<string> ResponseTypes { get; init; } = null!;
    public IReadOnlyCollection<string> GrantTypes { get; init; } = null!;
    public IReadOnlyCollection<string> Contacts { get; init; } = null!;
}