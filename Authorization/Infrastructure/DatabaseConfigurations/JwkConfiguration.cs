using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class JwkConfiguration : IEntityTypeConfiguration<Jwk>
{
  public void Configure(EntityTypeBuilder<Jwk> builder)
  {
    builder
      .Property(x => x.Exponent)
      .IsRequired();

    builder
      .Property(x => x.Modulus)
      .IsRequired();

    builder
      .Property(x => x.PrivateKey)
      .IsRequired();
  }
}