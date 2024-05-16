using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class PairwiseIdentifierConfiguration : IEntityTypeConfiguration<PairwiseIdentifier>
{
  public void Configure(EntityTypeBuilder<PairwiseIdentifier> builder)
  {
    builder
      .HasOne(x => x.Client)
      .WithMany(x => x.PairwiseIdentifiers)
      .IsRequired();

    builder
      .HasOne(x => x.User)
      .WithMany(x => x.PairwiseIdentifiers)
      .IsRequired();
  }
}
