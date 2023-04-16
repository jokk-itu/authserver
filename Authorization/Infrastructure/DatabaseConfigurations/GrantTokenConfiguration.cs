using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class GrantTokenConfiguration : IEntityTypeConfiguration<GrantToken>
{
  public void Configure(EntityTypeBuilder<GrantToken> builder)
  {
  }
}