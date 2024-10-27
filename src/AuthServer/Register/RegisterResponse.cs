using AuthServer.Enums;

namespace AuthServer.Register;

internal class RegisterResponse
{
	public required string ClientId { get; init; }
	public required long ClientIdIssuedAt { get; init; }
	public required string? ClientSecret { get; init; }
	public required long? ClientSecretExpiresAt { get; init; }
	public required string RegistrationAccessToken { get; init; }
	public required string RegistrationClientUri { get; init; }
	public required string ClientName { get; init; }
	public required ApplicationType ApplicationType { get; init; }
	public required SubjectType? SubjectType { get; init; }
	public required int? DefaultMaxAge { get; init; }
	public required TokenEndpointAuthMethod TokenEndpointAuthMethod { get; init; }
	public required SigningAlg TokenEndpointAuthSigningAlg { get; init; }
	public required string? Jwks { get; init; }
	public required string? JwksUri { get; init; }
	public required string? ClientUri { get; init; }
	public required string? PolicyUri { get; init; }
	public required string? TosUri { get; init; }
	public required string? InitiateLoginUri { get; init; }
	public required string? LogoUri { get; init; }
	public required string? BackchannelLogoutUri { get; init; }
	public required bool RequireSignedRequestObject { get; init; }
	public required bool RequireReferenceToken { get; init; }
	public required bool RequirePushedAuthorizationRequests { get; init; }
	public required EncryptionEnc? RequestObjectEncryptionEnc { get; init; }
	public required EncryptionAlg? RequestObjectEncryptionAlg { get; init; }
	public required SigningAlg? RequestObjectSigningAlg { get; init; }
	public required EncryptionEnc? UserinfoEncryptedResponseEnc { get; init; }
	public required EncryptionAlg? UserinfoEncryptedResponseAlg { get; init; }
	public required SigningAlg? UserinfoSignedResponseAlg { get; init; }
	public required EncryptionEnc? IdTokenEncryptedResponseEnc { get; init; }
	public required EncryptionAlg? IdTokenEncryptedResponseAlg { get; init; }
	public required SigningAlg? IdTokenSignedResponseAlg { get; init; }
	public required int? AuthorizationCodeExpiration { get; init; }
	public required int AccessTokenExpiration { get; init; }
	public required int? RefreshTokenExpiration { get; init; }
	public required int? ClientSecretExpiration { get; init; }
	public required int? JwksExpiration { get; init; }
	public required int? RequestUriExpiration { get; init; }
	public required IReadOnlyCollection<string> DefaultAcrValues { get; init; }
	public required IReadOnlyCollection<string> Scope { get; init; }
	public required IReadOnlyCollection<string> RedirectUris { get; init; }
	public required IReadOnlyCollection<string> PostLogoutRedirectUris { get; init; }
	public required IReadOnlyCollection<string> RequestUris { get; init; }
	public required IReadOnlyCollection<string> ResponseTypes { get; init; }
	public required IReadOnlyCollection<string> GrantTypes { get; init; }
	public required IReadOnlyCollection<string> Contacts { get; init; }
}