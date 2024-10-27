using AuthServer.Core;

namespace AuthServer.Entities;
public class Scope : Entity<int>
{
    public Scope(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private Scope() { }
#pragma warning restore

    public string Name { get; private init; }
    public ICollection<Client> Clients { get; private init; } = [];
    public ICollection<ConsentGrant> ConsentGrants { get; private init; } = [];
}