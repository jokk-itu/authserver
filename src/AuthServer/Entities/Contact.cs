using AuthServer.Core;

namespace AuthServer.Entities;
public class Contact : Entity<int>
{
    public Contact(string email, Client client)
    {
        Email = string.IsNullOrWhiteSpace(email) ? throw new ArgumentNullException(nameof(email)) : email;
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private Contact() { }
#pragma warning restore

    public string Email { get; private set; }
    public Client Client { get; private init; }
}