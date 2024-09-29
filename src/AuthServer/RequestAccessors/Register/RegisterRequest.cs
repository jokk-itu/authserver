﻿namespace AuthServer.RequestAccessors.Register;

public class RegisterRequest
{
    public required HttpMethod Method { get; set; }
    public string? ClientId { get; init; }
    public string? RegistrationAccessToken { get; init; }
    public string? ClientName { get; init; }
    public string? ApplicationType { get; init; }
    public string? SubjectType { get; init; }
    public string? DefaultMaxAge { get; init; }
    public string? TokenEndpointAuthMethod { get; init; }
    public string? TokenEndpointAuthSigningAlg { get; init; }
    public string? Jwks { get; init; }
    public string? JwksUri { get; init; }
    public string? ClientUri { get; init; }
    public string? PolicyUri { get; init; }
    public string? TosUri { get; init; }
    public string? InitiateLoginUri { get; init; }
    public string? LogoUri { get; init; }
    public string? BackchannelLogoutUri { get; init; }
    public string? RequireSignedRequestObject { get; init; }
    public string? RequireReferenceToken { get; init; }
    public string? RequirePushedAuthorizationRequests { get; init; }
    public string? RequestObjectEncryptionEnc { get; init; }
    public string? RequestObjectEncryptionAlg { get; init; }
    public string? RequestObjectSigningAlg { get; init; }
    public string? UserinfoEncryptedResponseEnc { get; init; }
    public string? UserinfoEncryptedResponseAlg { get; init; }
    public string? UserinfoSignedResponseAlg { get; init; }
    public string? IdTokenEncryptedResponseEnc { get; init; }
    public string? IdTokenEncryptedResponseAlg { get; init; }
    public string? IdTokenSignedResponseAlg { get; init; }
    public string? AuthorizationCodeExpiration { get; init; }
    public string? AccessTokenExpiration { get; init; }
    public string? RefreshTokenExpiration { get; init; }
    public string? ClientSecretExpiration { get; init; }
    public string? JwksExpiration { get; init; }
    public string? RequestUriExpiration { get; init; }
    public IReadOnlyCollection<string> DefaultAcrValues { get; init; } = [];
    public IReadOnlyCollection<string> Scope { get; init; } = [];
    public IReadOnlyCollection<string> RedirectUris { get; init; } = [];
    public IReadOnlyCollection<string> PostLogoutRedirectUris { get; init; } = [];
    public IReadOnlyCollection<string> RequestUris { get; init; } = [];
    public IReadOnlyCollection<string> ResponseTypes { get; init; } = [];
    public IReadOnlyCollection<string> GrantTypes { get; init; } = [];
    public IReadOnlyCollection<string> Contacts { get; init; } = [];
}