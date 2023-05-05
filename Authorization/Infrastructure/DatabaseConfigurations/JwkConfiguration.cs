using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class JwkConfiguration : IEntityTypeConfiguration<Jwk>
{
  public void Configure(EntityTypeBuilder<Jwk> builder)
  {
    throw new NotImplementedException();
  }
}
