using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class JwkConfiguration : IEntityTypeConfiguration<Jwk>
{
  public void Configure(EntityTypeBuilder<Jwk> builder)
  {
    builder.HasKey(jwk => jwk.KeyId);
    builder.ToTable("Jwks");
  }
}
