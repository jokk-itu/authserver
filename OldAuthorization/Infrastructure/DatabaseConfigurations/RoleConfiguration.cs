using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
  public void Configure(EntityTypeBuilder<Role> builder)
  {
    builder
      .Property(x => x.Value)
      .IsRequired();

    builder
      .HasIndex(x => x.Value)
      .IsUnique();
  }
}
