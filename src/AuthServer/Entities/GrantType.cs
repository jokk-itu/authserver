using AuthServer.Core;

namespace AuthServer.Entities;
public class GrantType : Entity<int>
{
    public GrantType(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private GrantType() { }
#pragma warning restore

    public string Name { get; private init; }
    public ICollection<Client> Clients { get; private init; } = [];
}