using AuthServer.Enums;

namespace AuthServer.Cache.Entities;
internal class CachedClient
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string? SecretHash { get; init; }
    public required DateTime? SecretExpiresAt { get; init; }
    public required int? SecretExpiration { get; init; }
    public required int AccessTokenExpiration { get; init; }
    public required int? RefreshTokenExpiration { get; init; }
    public required int? AuthorizationCodeExpiration { get; init; }
    public required int? JwksExpiration { get; init; }
    public required int? RequestUriExpiration { get; init; }
    public required string? JwksUri { get; set; }
    public required string? Jwks { get; init; }
    public required DateTime? JwksExpiresAt { get; init; }
    public required string? TosUri { get; init; }
    public required string? PolicyUri { get; init; }
    public required string? ClientUri { get; init; }
    public required string? LogoUri { get; init; }
    public required string? InitiateLoginUri { get; init; }
    public required string? BackchannelLogoutUri { get; init; }
    public required bool RequireReferenceToken { get; init; }
    public required bool RequireConsent { get; init; }
    public required bool RequireSignedRequestObject { get; init; }
    public required bool RequirePushedAuthorizationRequests { get; init; }
    public required int? DefaultMaxAge { get; init; }
    public required ApplicationType ApplicationType { get; init; }
    public required TokenEndpointAuthMethod TokenEndpointAuthMethod { get; init; }
    public required SubjectType? SubjectType { get; init; }

    public required SigningAlg TokenEndpointAuthSigningAlg { get; set; }

    public required EncryptionEnc? UserinfoEncryptedResponseEnc { get; set; }
    public required EncryptionAlg? UserinfoEncryptedResponseAlg { get; set; }
    public required SigningAlg? UserinfoSignedResponseAlg { get; set; }

    public required EncryptionEnc? RequestObjectEncryptionEnc { get; set; }
    public required EncryptionAlg? RequestObjectEncryptionAlg { get; set; }
    public required SigningAlg? RequestObjectSigningAlg { get; set; }

    public required EncryptionEnc? IdTokenEncryptedResponseEnc { get; set; }
    public required EncryptionAlg? IdTokenEncryptedResponseAlg { get; set; }
    public required SigningAlg? IdTokenSignedResponseAlg { get; set; }

    public required IReadOnlyCollection<string> Scopes { get; init; }
    public required IReadOnlyCollection<string> GrantTypes { get; init; }
    public required IReadOnlyCollection<string> ResponseTypes { get; init; }
    public required IReadOnlyCollection<string> PostLogoutRedirectUris { get; init; }
    public required IReadOnlyCollection<string> RedirectUris { get; init; }
    public required IReadOnlyCollection<string> RequestUris { get; init; }
    public required IReadOnlyCollection<string> DefaultAcrValues { get; init; }
}