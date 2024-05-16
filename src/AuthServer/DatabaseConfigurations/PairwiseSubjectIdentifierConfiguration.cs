using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class PairwiseSubjectIdentifierConfiguration : IEntityTypeConfiguration<PairwiseSubjectIdentifier>
{
    public void Configure(EntityTypeBuilder<PairwiseSubjectIdentifier> builder)
    {
        builder
            .HasOne(x => x.PublicSubjectIdentifier)
            .WithMany(x => x.PairwiseSubjectIdentifiers)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.PairwiseSubjectIdentifiers)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}