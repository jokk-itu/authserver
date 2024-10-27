namespace AuthServer.Entities;

public class ClientAuthenticationContextReference
{
    public ClientAuthenticationContextReference(Client client,
        AuthenticationContextReference authenticationContextReference, int order)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        AuthenticationContextReference = authenticationContextReference ?? throw new ArgumentNullException(nameof(authenticationContextReference));
        Order = order < 0 ? throw new ArgumentException("order must not be a negative number") : order;
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private ClientAuthenticationContextReference() { }
#pragma warning restore

    public int Order { get; private init; }
    public Client Client { get; private init; }
    public string ClientId { get; private init; } = null!;

    public AuthenticationContextReference AuthenticationContextReference { get; private init; }
    public int AuthenticationContextReferenceId { get; private init; }
}
