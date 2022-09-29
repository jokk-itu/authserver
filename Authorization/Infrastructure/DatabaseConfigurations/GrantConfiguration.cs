using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class GrantConfiguration : IEntityTypeConfiguration<Grant>
{
  public void Configure(EntityTypeBuilder<Grant> builder)
  {
    builder
      .HasMany(x => x.Scopes)
      .WithMany(x => x.Grants)
      .UsingEntity(x => x.ToTable("GrantScopes"));

    builder.ToTable("Grants");
  }
}
