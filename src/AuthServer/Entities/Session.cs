using System.Linq.Expressions;
using AuthServer.Core;

namespace AuthServer.Entities;
public class Session : Entity<string>
{
    public Session(SubjectIdentifier subjectIdentifier)
    {
        Id = Guid.NewGuid().ToString();
        SubjectIdentifier = subjectIdentifier ?? throw new ArgumentNullException(nameof(subjectIdentifier));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private Session() { }
#pragma warning restore

    public DateTime? RevokedAt { get; private set; }
    public SubjectIdentifier SubjectIdentifier { get; private init; }
    public ICollection<AuthorizationGrant> AuthorizationGrants { get; private init; } = [];

    public static Expression<Func<Session, bool>> IsActive = s => s.RevokedAt == null;

    public void Revoke()
    {
        RevokedAt ??= DateTime.UtcNow;
    }
}