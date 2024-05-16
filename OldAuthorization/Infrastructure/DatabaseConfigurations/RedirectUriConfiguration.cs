using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class RedirectUriConfiguration : IEntityTypeConfiguration<RedirectUri>
{
  public void Configure(EntityTypeBuilder<RedirectUri> builder)
  {
    builder
      .Property(x => x.Uri)
      .IsRequired();

    builder
      .Property(x => x.Type)
      .HasConversion<string>();
  }
}