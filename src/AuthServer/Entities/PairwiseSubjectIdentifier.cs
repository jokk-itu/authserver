using AuthServer.Enums;

namespace AuthServer.Entities;
public class PairwiseSubjectIdentifier : SubjectIdentifier
{
    public PairwiseSubjectIdentifier(Client client, PublicSubjectIdentifier publicSubjectIdentifier)
        : base(SubjectType.Pairwise)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        PublicSubjectIdentifier = publicSubjectIdentifier ?? throw new ArgumentNullException(nameof(publicSubjectIdentifier));
    }

#pragma warning disable CS8618
    // Used to hydrate EF Core model
    private PairwiseSubjectIdentifier() { }
#pragma warning restore

    public Client Client { get; init; }
    public PublicSubjectIdentifier PublicSubjectIdentifier { get; init; }
}