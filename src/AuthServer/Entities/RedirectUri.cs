using AuthServer.Core;

namespace AuthServer.Entities;

public class RedirectUri : Entity<int>
{
    public RedirectUri(string uri, Client client)
    {
        Uri = string.IsNullOrWhiteSpace(uri) ? throw new ArgumentNullException(nameof(uri)) : uri;
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private RedirectUri() { }
#pragma warning restore

    public string Uri { get; set; }
    public Client Client { get; private init; }
}