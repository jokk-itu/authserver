using System.Linq.Expressions;
using AuthServer.Core;

namespace AuthServer.Entities;
public class Session : Entity<string>
{
    public Session(PublicSubjectIdentifier publicSubjectIdentifier)
    {
        Id = Guid.NewGuid().ToString();
        PublicSubjectIdentifier = publicSubjectIdentifier ?? throw new ArgumentNullException(nameof(publicSubjectIdentifier));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected Session() { }
#pragma warning restore

    public DateTime? RevokedAt { get; private set; }
    public PublicSubjectIdentifier PublicSubjectIdentifier { get; private init; }
    public ICollection<AuthorizationGrant> AuthorizationGrants { get; private init; } = [];

    public static Expression<Func<Session, bool>> IsActive = s => s.RevokedAt == null;

    public void Revoke()
    {
        RevokedAt ??= DateTime.UtcNow;
    }
}