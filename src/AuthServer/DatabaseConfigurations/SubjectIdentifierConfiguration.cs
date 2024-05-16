using AuthServer.Entities;
using AuthServer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class SubjectIdentifierConfiguration : IEntityTypeConfiguration<SubjectIdentifier>
{
  public void Configure(EntityTypeBuilder<SubjectIdentifier> builder)
  {
      builder
          .HasDiscriminator(x => x.Type)
          .HasValue<PairwiseSubjectIdentifier>(SubjectType.Pairwise)
          .HasValue<PublicSubjectIdentifier>(SubjectType.Public);
  }
}