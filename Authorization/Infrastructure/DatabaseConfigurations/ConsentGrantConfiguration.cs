using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class ConsentGrantConfiguration : IEntityTypeConfiguration<ConsentGrant>
{
  public void Configure(EntityTypeBuilder<ConsentGrant> builder)
  {
    builder
      .HasMany(x => x.ConsentedClaims)
      .WithMany(x => x.ConsentGrants)
      .UsingEntity(x => x.ToTable("ConsentedGrantClaims"));

    builder
      .HasMany(x => x.ConsentedScopes)
      .WithMany(x => x.ConsentGrants)
      .UsingEntity(x => x.ToTable("ConsentedGrantScopes"));

    builder.ToTable("ConsentGrants");
  }
}
