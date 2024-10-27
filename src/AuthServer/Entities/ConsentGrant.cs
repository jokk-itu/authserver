using AuthServer.Core;

namespace AuthServer.Entities;
public class ConsentGrant : Entity<int>
{
    public ConsentGrant(SubjectIdentifier subjectIdentifier, Client client)
    {
        SubjectIdentifier = subjectIdentifier ?? throw new ArgumentNullException(nameof(subjectIdentifier));
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private ConsentGrant() { }
#pragma warning restore

    public Client Client { get; private init; }
    public SubjectIdentifier SubjectIdentifier { get; private init; }
    public ICollection<Claim> ConsentedClaims { get; private init; } = [];
    public ICollection<Scope> ConsentedScopes { get; private init; } = [];
}