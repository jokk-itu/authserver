using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class IdTokenConfiguration : IEntityTypeConfiguration<IdToken>
{
  public void Configure(EntityTypeBuilder<IdToken> builder)
  {
    builder
      .HasOne(t => t.Client)
      .WithMany(c => c.IdTokens)
      .OnDelete(DeleteBehavior.Restrict);

    builder
      .HasOne(t => t.Session)
      .WithMany(s => s.IdTokens)
      .OnDelete(DeleteBehavior.Restrict);
  }
}