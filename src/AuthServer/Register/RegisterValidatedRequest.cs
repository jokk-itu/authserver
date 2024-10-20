using AuthServer.Enums;

namespace AuthServer.Register;

internal class RegisterValidatedRequest
{
    public HttpMethod Method { get; init; } = null!;
    public string ClientId { get; set; } = null!;
    public string RegistrationAccessToken { get; set; } = null!;
    public string ClientName { get; set; } = null!;
	public ApplicationType ApplicationType { get; set; }
	public SubjectType? SubjectType { get; set; }
	public int? DefaultMaxAge { get; set; }
	public TokenEndpointAuthMethod TokenEndpointAuthMethod { get; set; }
	public SigningAlg TokenEndpointAuthSigningAlg { get; set; }
	public string? Jwks { get; set; }
	public string? JwksUri { get; set; }
	public string? ClientUri { get; set; }
	public string? PolicyUri { get; set; }
	public string? TosUri { get; set; }
	public string? InitiateLoginUri { get; set; }
	public string? LogoUri { get; set; }
	public string? BackchannelLogoutUri { get; set; }
	public string? SectorIdentifierUri { get; set; }
	public bool RequireSignedRequestObject { get; set; }
	public bool RequireReferenceToken { get; set; }
	public bool RequirePushedAuthorizationRequests { get; set; }
	public EncryptionEnc? RequestObjectEncryptionEnc { get; set; }
	public EncryptionAlg? RequestObjectEncryptionAlg { get; set; }
	public SigningAlg? RequestObjectSigningAlg { get; set; }
	public EncryptionEnc? UserinfoEncryptedResponseEnc { get; set; }
	public EncryptionAlg? UserinfoEncryptedResponseAlg { get; set; }
	public SigningAlg? UserinfoSignedResponseAlg { get; set; }
	public EncryptionEnc? IdTokenEncryptedResponseEnc { get; set; }
	public EncryptionAlg? IdTokenEncryptedResponseAlg { get; set; }
	public SigningAlg? IdTokenSignedResponseAlg { get; set; }
	public int? AuthorizationCodeExpiration { get; set; }
	public int AccessTokenExpiration { get; set; }
	public int? RefreshTokenExpiration { get; set; }
	public int? ClientSecretExpiration { get; set; }
	public int? JwksExpiration { get; set; }
	public int? RequestUriExpiration { get; set; }
	public IReadOnlyCollection<string> DefaultAcrValues { get; set; } = [];
    public IReadOnlyCollection<string> Scope { get; set; } = [];
    public IReadOnlyCollection<string> RedirectUris { get; set; } = [];
    public IReadOnlyCollection<string> PostLogoutRedirectUris { get; set; } = [];
    public IReadOnlyCollection<string> RequestUris { get; set; } = [];
    public IReadOnlyCollection<string> ResponseTypes { get; set; } = [];
    public IReadOnlyCollection<string> GrantTypes { get; set; } = [];
    public IReadOnlyCollection<string> Contacts { get; set; } = [];
}