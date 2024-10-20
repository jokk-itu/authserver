using AuthServer.Core;
using AuthServer.Helpers;

namespace AuthServer.Entities;
public class SectorIdentifier : Entity<int>
{
    public SectorIdentifier(string uri)
    {
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        Salt = CryptographyHelper.GetRandomString(32);
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private SectorIdentifier() { }
#pragma warning restore

    public string Uri { get; private init; }
    public string Salt { get; private init; }

    public ICollection<Client> Clients { get; private init; } = [];

    public string GetHostComponent() => new Uri(Uri).Host;
}
