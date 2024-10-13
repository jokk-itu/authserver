using System.Linq.Expressions;
using AuthServer.Core;

namespace AuthServer.Entities;
public class AuthorizationGrant : Entity<string>
{
    public AuthorizationGrant(Session session, Client client, SubjectIdentifier subjectIdentifier, AuthenticationContextReference authenticationContextReference)
    {
        Id = Guid.NewGuid().ToString();
        AuthTime = DateTime.UtcNow;
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Client = client ?? throw new ArgumentNullException(nameof(client));
        SubjectIdentifier = subjectIdentifier ?? throw new ArgumentNullException(nameof(subjectIdentifier));
        AuthenticationContextReference = authenticationContextReference ?? throw new ArgumentNullException(nameof(authenticationContextReference));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private AuthorizationGrant(){}
#pragma warning restore

    public DateTime AuthTime { get; private init; }
    public DateTime? RevokedAt { get; private set; }
    public Session Session { get; private init; }
    public Client Client { get; private init; }
    public SubjectIdentifier SubjectIdentifier { get; private init; }
    public AuthenticationContextReference AuthenticationContextReference { get; private init; }
    public ICollection<AuthorizationCode> AuthorizationCodes { get; init; } = [];
    public ICollection<Nonce> Nonces { get; init; } = [];
    public ICollection<GrantToken> GrantTokens { get; init; } = [];
    public ICollection<AuthenticationMethodReference> AuthenticationMethodReferences { get; init; } = [];

    public void Revoke()
    {
        RevokedAt ??= DateTime.UtcNow;
    }

    public static readonly Expression<Func<AuthorizationGrant, bool>> IsActive = a => a.RevokedAt == null;
}