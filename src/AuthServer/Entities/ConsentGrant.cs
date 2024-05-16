using AuthServer.Core;

namespace AuthServer.Entities;
public class ConsentGrant : Entity<int>
{
    public ConsentGrant(PublicSubjectIdentifier publicSubjectIdentifier, Client client)
    {
        PublicSubjectIdentifier = publicSubjectIdentifier ?? throw new ArgumentNullException(nameof(publicSubjectIdentifier));
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    protected ConsentGrant() { }
#pragma warning restore

    public Client Client { get; private init; }
    public PublicSubjectIdentifier PublicSubjectIdentifier { get; private init; }
    public ICollection<Claim> ConsentedClaims { get; private init; } = [];
    public ICollection<Scope> ConsentedScopes { get; private init; } = [];
}