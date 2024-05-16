using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class AuthorizationCodeGrantConfiguration : IEntityTypeConfiguration<AuthorizationCodeGrant>
{
  public void Configure(EntityTypeBuilder<AuthorizationCodeGrant> builder)
  {
    builder
      .HasMany(x => x.GrantTokens)
      .WithOne(x => x.AuthorizationGrant)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasMany(x => x.Nonces)
      .WithOne(x => x.AuthorizationCodeGrant)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasMany(x => x.AuthorizationCodes)
      .WithOne(x => x.AuthorizationCodeGrant)
      .OnDelete(DeleteBehavior.Cascade);
  }
}