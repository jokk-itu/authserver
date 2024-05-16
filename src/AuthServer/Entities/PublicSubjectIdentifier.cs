using AuthServer.Enums;

namespace AuthServer.Entities;
public class PublicSubjectIdentifier : SubjectIdentifier 
{
    public PublicSubjectIdentifier()
        : base(SubjectType.Public)
    {
    }

    public ICollection<Session> Sessions { get; private init; } = [];
    public ICollection<ConsentGrant> ConsentGrants { get; private init; } = [];
    public ICollection<PairwiseSubjectIdentifier> PairwiseSubjectIdentifiers { get; private init;} = [];
}