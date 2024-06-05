using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class ConsentGrantConfiguration : IEntityTypeConfiguration<ConsentGrant>
{
  public void Configure(EntityTypeBuilder<ConsentGrant> builder)
  {
    builder
      .HasMany(x => x.ConsentedClaims)
      .WithMany(x => x.ConsentGrants);

    builder
      .HasMany(x => x.ConsentedScopes)
      .WithMany(x => x.ConsentGrants);

    builder
        .HasOne(x => x.Client)
        .WithMany(x => x.ConsentGrants)
        .IsRequired()
        .OnDelete(DeleteBehavior.ClientCascade);

    builder
        .HasOne(x => x.PublicSubjectIdentifier)
        .WithMany(x => x.ConsentGrants)
        .IsRequired()
        .OnDelete(DeleteBehavior.ClientCascade);
  }
}
