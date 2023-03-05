using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
  public void Configure(EntityTypeBuilder<Session> builder)
  {
    builder
      .HasMany(x => x.AuthorizationCodeGrants)
      .WithOne(x => x.Session)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasOne(x => x.User)
      .WithMany(x => x.Sessions)
      .OnDelete(DeleteBehavior.NoAction);

    builder.ToTable("Sessions");
  }
}