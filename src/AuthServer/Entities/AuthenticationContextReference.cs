using AuthServer.Core;

namespace AuthServer.Entities;

public class AuthenticationContextReference : Entity<int>
{
    public AuthenticationContextReference(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private AuthenticationContextReference() { }
#pragma warning restore

    public string Name { get; private init; }
    public ICollection<ClientAuthenticationContextReference> ClientAuthenticationContextReferences { get; private init; } = [];
    public ICollection<AuthorizationGrant> AuthorizationGrants { get; private init; } = [];
}
