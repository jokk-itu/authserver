using AuthServer.Core;

namespace AuthServer.Entities;
public class ResponseType : Entity<int>
{
    public ResponseType(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private ResponseType() { }
#pragma warning restore

    public string Name { get; private init; }
    public ICollection<Client> Clients { get; private init; } = [];
}