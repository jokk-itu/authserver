namespace AuthServer.RequestAccessors.Register;

public abstract class RegisterRequest
{
    public required string ClientName { get; init; }
    public required string ApplicationType { get; init; }
    public required string SubjectType { get; init; }
    public required string DefaultMaxAge { get; init; }
    public required string TokenEndpointAuthMethod { get; init; }
    public required string TokenEndpointAuthSigningAlg { get; init; }
    public required string Jwks { get; init; }
    public required string JwksUri { get; init; }
    public required string ClientUri { get; init; }
    public required string PolicyUri { get; init; }
    public required string TosUri { get; init; }
    public required string InitiateLoginUri { get; init; }
    public required string LogoUri { get; init; }
    public required string BackchannelLogoutUri { get; init; }
    public required string RequireSignedRequestObject { get; init; }
    public required string RequestObjectEncryptionEnc { get; init; }
    public required string RequestObjectEncryptionAlg { get; init; }
    public required string RequestObjectSigningAlg { get; init; }
    public required string UserinfoEncryptedResponseEnc { get; init; }
    public required string UserinfoEncryptedResponseAlg { get; init; }
    public required string UserinfoSignedResponseAlg { get; init; }
    public required string IdTokenEncryptedResponseEnc { get; init; }
    public required string IdTokenEncryptedResponseAlg { get; init; }
    public required string IdTokenSignedResponseAlg { get; init; }
    public required string AuthorizationEncryptedResponseEnc { get; init; }
    public required string AuthorizationEncryptedResponseAlg { get; init; }
    public required string AuthorizationSignedResponseAlg { get; init; }
    public required string AuthorizationCodeExpiration { get; init; }
    public required string AccessTokenExpiration { get; init; }
    public required string RefreshTokenExpiration { get; init; }
    public required string ClientSecretExpiration { get; init; }
    public required string JwksExpiration { get; init; }
    public required IReadOnlyCollection<string> DefaultAcrValues { get; init; }
    public required IReadOnlyCollection<string> Scope { get; init; }
    public required IReadOnlyCollection<string> RedirectUris { get; init; }
    public required IReadOnlyCollection<string> PostLogoutRedirectUris { get; init; }
    public required IReadOnlyCollection<string> RequestUris { get; init; }
    public required IReadOnlyCollection<string> ResponseTypes { get; init; }
    public required IReadOnlyCollection<string> GrantTypes { get; init; }
    public required IReadOnlyCollection<string> Contacts { get; init; }
}