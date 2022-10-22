using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
  public void Configure(EntityTypeBuilder<RefreshToken> builder)
  {
    builder
      .HasOne(t => t.Client)
      .WithMany(c => c.RefreshTokens)
      .OnDelete(DeleteBehavior.Restrict);

    builder
      .HasOne(t => t.Session)
      .WithMany(s => s.RefreshTokens)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
