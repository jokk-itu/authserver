using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class GrantConfiguration : IEntityTypeConfiguration<Grant>
{
  public void Configure(EntityTypeBuilder<Grant> builder)
  {
    builder.ToTable("ClientGrants");
  }
}
