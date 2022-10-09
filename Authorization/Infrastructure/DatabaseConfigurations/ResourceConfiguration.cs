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
      .WithMany(scope => scope.Resources)
      .UsingEntity(link => link.ToTable("ResourceScopes"));

    builder
      .HasMany(x => x.ResourceRegistrationTokens)
      .WithOne(x => x.Resource);

    builder.ToTable("Resources");
  }
}