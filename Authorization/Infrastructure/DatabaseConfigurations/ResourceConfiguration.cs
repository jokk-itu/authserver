using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
  public void Configure(EntityTypeBuilder<Resource> builder)
  {
    builder
      .HasMany(resource => resource.Scopes)
      .WithMany(scope => scope.Resources);

    builder
      .Property(x => x.Name)
      .IsRequired();

    builder
      .Property(x => x.Secret)
      .IsRequired();

    builder
      .Property(r => r.Uri)
      .IsRequired();

    builder
      .HasIndex(r => r.Name)
      .IsUnique();

    builder
      .HasIndex(r => r.Uri)
      .IsUnique();
  }
}