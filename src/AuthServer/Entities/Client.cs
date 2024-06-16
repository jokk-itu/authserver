using AuthServer.Core;
using AuthServer.Enums;

namespace AuthServer.Entities;
public class Client : Entity<string>
{
    public Client(string name, ApplicationType applicationType, TokenEndpointAuthMethod tokenEndpointAuthMethod, long? defaultMaxAge = null)
    {
        Id = Guid.NewGuid().ToString();
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
        ApplicationType = applicationType;
        TokenEndpointAuthMethod = tokenEndpointAuthMethod;
        DefaultMaxAge = defaultMaxAge is null or >= 0 ? defaultMaxAge : throw new ArgumentException("Must be zero or a positive number", nameof(defaultMaxAge));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected Client() { }
#pragma warning restore

    /// <summary>
    /// Name of client
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Secret of client
    /// </summary>
    public string? SecretHash { get; private set; }

    /// <summary>
    /// Secret expiration
    /// </summary>
    public DateTime? SecretExpiresAt { get; private set; }

    /// <summary>
    /// Secret lifetime in seconds
    /// </summary>
    public int? SecretExpiration { get; set; }

    /// <summary>
    /// Access token lifetime in seconds
    /// </summary>
    public int AccessTokenExpiration { get; set; }

    /// <summary>
    /// Refresh token lifetime in seconds
    /// </summary>
    public int RefreshTokenExpiration { get; set; }

    /// <summary>
    /// Authorization code lifetime in seconds
    /// </summary>
    public int AuthorizationCodeExpiration { get; set; }

    /// <summary>
    /// Lifetime of cached jwks fetched from jwks_uri in seconds. 
    /// </summary>
    public int JwksExpiration { get; set; }

    /// <summary>
    /// Uri to terms of service page
    /// </summary>
    public string? TosUri { get; set; }

    /// <summary>
    /// Uri to policy page
    /// </summary>
    public string? PolicyUri { get; set; }

    /// <summary>
    /// Uri to client
    /// </summary>
    public string? ClientUri { get; set; }

    /// <summary>
    /// Uri to logo of client
    /// </summary>
    public string? LogoUri { get; set; }

    /// <summary>
    /// Uri which the identity provider can invoke to start authentication flow
    /// </summary>
    public string? InitiateLoginUri { get; set; }

    /// <summary>
    /// Uri which the identity provider can invoke to log out the client
    /// </summary>
    public string? BackchannelLogoutUri { get; set; }

    /// <summary>
    /// Uri to fetch a JWK set
    /// </summary>
    public string? JwksUri { get; set; }

    /// <summary>
    /// JSON array of a JWK set
    /// </summary>
    public string? Jwks { get; set; }

    /// <summary>
    /// Managed by the authorization server, to determine when to refresh jwks from jwks_uri.
    /// </summary>
    public DateTime? JwksExpiresAt { get; set; }

    /// <summary>
    /// All tokens sent to the client are reference tokens
    /// </summary>
    public bool RequireReferenceToken { get; set; }

    /// <summary>
    /// All authorization attempts involving an end user requires an active consent grant
    /// </summary>
    public bool RequireConsent { get; set; }

    /// <summary>
    /// Maximum age since last authentication by end user in epoch format
    /// Zero means it always requires authentication by end user
    /// </summary>
    public long? DefaultMaxAge { get; set; }

    /// <summary>
    /// Type of the client
    /// </summary>
    public ApplicationType ApplicationType { get; set; }

    /// <summary>
    /// Method of client authentication which is used at the token endpoint
    /// </summary>
    public TokenEndpointAuthMethod TokenEndpointAuthMethod { get; set; }

    /// <summary>
    /// Type of subject identifier belonging to end users authenticating
    /// </summary>
    public SubjectType? SubjectType { get; set; }

    public SigningAlg? AuthorizationSignedResponseAlg { get; set; }
    public EncryptionAlg? AuthorizationEncryptedResponseAlg { get; set; }
    public EncryptionEnc? AuthorizationEncryptedResponseEnc { get; set; }

    /// <summary>
    /// Algorithm used to sign tokens for client authentication private_key_jwt.
    /// </summary>
    public SigningAlg TokenEndpointAuthSigningAlg { get; set; }

    public EncryptionEnc? UserinfoEncryptedResponseEnc { get; set; }
    public EncryptionAlg? UserinfoEncryptedResponseAlg { get; set; }
    public SigningAlg? UserinfoSignedResponseAlg { get; set; }

    public EncryptionEnc? RequestObjectEncryptionEnc { get; set; }
    public EncryptionAlg? RequestObjectEncryptionAlg { get; set; }
    public SigningAlg? RequestObjectSigningAlg { get; set; }

    public EncryptionEnc? IdTokenEncryptedResponseEnc { get; set; }
    public EncryptionAlg? IdTokenEncryptedResponseAlg { get; set; }
    public SigningAlg IdTokenSignedResponseAlg { get; set; }

    
    public ICollection<RedirectUri> RedirectUris { get; private init; } = [];
    public ICollection<PostLogoutRedirectUri> PostLogoutRedirectUris { get; private init; } = [];
    public ICollection<RequestUri> RequestUris { get; private init; } = [];
    public ICollection<GrantType> GrantTypes { get; private init; } = [];
    public ICollection<ConsentGrant> ConsentGrants { get; private init; } = [];
    public ICollection<Scope> Scopes { get; private init; } = [];
    public ICollection<Contact> Contacts { get; private init; } = [];
    public ICollection<ResponseType> ResponseTypes { get; private init; } = [];
    public ICollection<AuthorizationGrant> AuthorizationGrants { get; private init; } = [];
    public ICollection<ClientToken> ClientTokens { get; private init; } = [];
    public ICollection<PairwiseSubjectIdentifier> PairwiseSubjectIdentifiers { get; private init; } = [];

    public void SetSecret(string secretHash)
    {
        SecretHash = secretHash;
        if (SecretExpiration is not null)
        {
            SecretExpiresAt = DateTime.UtcNow.AddSeconds(SecretExpiration.Value);
        }
        else
        {
            SecretExpiresAt = null;
        }
    }
}