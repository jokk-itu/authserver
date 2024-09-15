using AuthServer.Core;

namespace AuthServer.Entities;
public class Claim : Entity<int>
{
    public Claim(string name)
    {
        Name = name;
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private Claim() { }
#pragma warning restore

    public string Name { get; private init; }
    public ICollection<ConsentGrant> ConsentGrants { get; private init; } = [];
}