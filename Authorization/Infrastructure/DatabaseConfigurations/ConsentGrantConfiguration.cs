using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ConsentGrantConfiguration : IEntityTypeConfiguration<ConsentGrant>
{
  public void Configure(EntityTypeBuilder<ConsentGrant> builder)
  {
    builder
      .HasMany(x => x.ConsentedClaims)
      .WithMany(x => x.ConsentGrants);

    builder
      .HasMany(x => x.ConsentedScopes)
      .WithMany(x => x.ConsentGrants);
  }
}
