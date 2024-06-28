using AuthServer.Enums;
using System.Text.Json.Serialization;
using AuthServer.Core;

namespace AuthServer.Endpoints.Responses;

internal class RegisterResponse
{
	[JsonPropertyName(Parameter.ClientId)]
	public required string ClientId { get; init; }

	[JsonPropertyName(Parameter.ClientIdIssuedAt)]
	public DateTime ClientIdIssuedAt { get; init; }

	[JsonPropertyName(Parameter.ClientSecret)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ClientSecret { get; init; }

	[JsonPropertyName(Parameter.ClientSecretExpiresAt)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public long? ClientSecretExpiresAt { get; init; }

	[JsonPropertyName(Parameter.RegistrationAccessToken)]
	public required string RegistrationAccessToken { get; init; }

	[JsonPropertyName(Parameter.RegistrationClientUri)]
	public required string RegistrationClientUri { get; init; }

	[JsonPropertyName(Parameter.ClientName)]
	public required string ClientName { get; init; }

	[JsonPropertyName(Parameter.ApplicationType)]
	public required ApplicationType ApplicationType { get; init; }

	[JsonPropertyName(Parameter.SubjectType)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SubjectType? SubjectType { get; init; }

	[JsonPropertyName(Parameter.DefaultMaxAge)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? DefaultMaxAge { get; init; }

	[JsonPropertyName(Parameter.TokenEndpointAuthMethod)]
	public required TokenEndpointAuthMethod TokenEndpointAuthMethod { get; init; }

	[JsonPropertyName(Parameter.TokenEndpointAuthSigningAlg)]
	public required SigningAlg TokenEndpointAuthSigningAlg { get; init; }

	[JsonPropertyName(Parameter.Jwks)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Jwks { get; init; }

	[JsonPropertyName(Parameter.JwksUri)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? JwksUri { get; init; }

	[JsonPropertyName(Parameter.ClientUri)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? ClientUri { get; init; }

	[JsonPropertyName(Parameter.PolicyUri)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? PolicyUri { get; init; }

	[JsonPropertyName(Parameter.TosUri)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? TosUri { get; init; }

	[JsonPropertyName(Parameter.InitiateLoginUri)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? InitiateLoginUri { get; init; }

	[JsonPropertyName(Parameter.LogoUri)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? LogoUri { get; init; }

	[JsonPropertyName(Parameter.BackchannelLogoutUri)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? BackchannelLogoutUri { get; init; }

	[JsonPropertyName(Parameter.RequireSignedRequestObject)]
	public required bool RequireSignedRequestObject { get; init; }

	[JsonPropertyName(Parameter.RequireReferenceToken)]
	public required bool RequireReferenceToken { get; init; }

	[JsonPropertyName(Parameter.RequestObjectEncryptionEnc)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EncryptionEnc? RequestObjectEncryptionEnc { get; init; }

	[JsonPropertyName(Parameter.RequestObjectEncryptionAlg)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EncryptionAlg? RequestObjectEncryptionAlg { get; init; }

	[JsonPropertyName(Parameter.RequestObjectSigningAlg)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SigningAlg? RequestObjectSigningAlg { get; init; }

	[JsonPropertyName(Parameter.UserinfoEncryptedResponseEnc)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EncryptionEnc? UserinfoEncryptedResponseEnc { get; init; }

	[JsonPropertyName(Parameter.UserinfoEncryptedResponseAlg)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EncryptionAlg? UserinfoEncryptedResponseAlg { get; init; }

	[JsonPropertyName(Parameter.UserinfoSignedResponseAlg)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SigningAlg? UserinfoSignedResponseAlg { get; init; }

	[JsonPropertyName(Parameter.IdTokenEncryptedResponseEnc)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EncryptionEnc? IdTokenEncryptedResponseEnc { get; init; }

	[JsonPropertyName(Parameter.IdTokenEncryptedResponseAlg)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EncryptionAlg? IdTokenEncryptedResponseAlg { get; init; }

	[JsonPropertyName(Parameter.IdTokenSignedResponseAlg)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SigningAlg? IdTokenSignedResponseAlg { get; init; }

	[JsonPropertyName(Parameter.AuthorizationEncryptedResponseEnc)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EncryptionEnc? AuthorizationEncryptedResponseEnc { get; init; }

	[JsonPropertyName(Parameter.AuthorizationEncryptedResponseAlg)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EncryptionAlg? AuthorizationEncryptedResponseAlg { get; init; }

	[JsonPropertyName(Parameter.AuthorizationSignedResponseAlg)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SigningAlg? AuthorizationSignedResponseAlg { get; init; }

	[JsonPropertyName(Parameter.AuthorizationCodeExpiration)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? AuthorizationCodeExpiration { get; init; }

	[JsonPropertyName(Parameter.AccessTokenExpiration)]
	public required int AccessTokenExpiration { get; init; }

	[JsonPropertyName(Parameter.RefreshTokenExpiration)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? RefreshTokenExpiration { get; init; }

	[JsonPropertyName(Parameter.ClientSecretExpiration)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? ClientSecretExpiration { get; init; }

	[JsonPropertyName(Parameter.JwksExpiration)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? JwksExpiration { get; init; }

	[JsonPropertyName(Parameter.DefaultAcrValues)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyCollection<string>? DefaultAcrValues { get; init; }

	[JsonPropertyName(Parameter.Scope)]
	public required IReadOnlyCollection<string> Scope { get; init; }

	[JsonPropertyName(Parameter.RedirectUris)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyCollection<string>? RedirectUris { get; init; }

	[JsonPropertyName(Parameter.PostLogoutRedirectUris)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyCollection<string>? PostLogoutRedirectUris { get; init; }

	[JsonPropertyName(Parameter.RequestUris)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyCollection<string>? RequestUris { get; init; }

	[JsonPropertyName(Parameter.ResponseTypes)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyCollection<string>? ResponseTypes { get; init; }

	[JsonPropertyName(Parameter.GrantTypes)]
	public required IReadOnlyCollection<string> GrantTypes { get; init; }

	[JsonPropertyName(Parameter.Contacts)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyCollection<string>? Contacts { get; init; }
}