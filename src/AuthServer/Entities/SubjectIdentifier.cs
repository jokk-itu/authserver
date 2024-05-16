using AuthServer.Core;
using AuthServer.Enums;

namespace AuthServer.Entities;

public abstract class SubjectIdentifier : Entity<string>
{
    protected SubjectIdentifier(SubjectType type)
    {
        Id = Guid.NewGuid().ToString();
        Type = type;
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected SubjectIdentifier() { }
#pragma warning restore

    public SubjectType Type { get; private init; }
    public ICollection<AuthorizationGrant> AuthorizationGrants { get; private init; } = [];
}