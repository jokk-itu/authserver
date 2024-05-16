using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class SessionConfiguration : IEntityTypeConfiguration<Session>
{
  public void Configure(EntityTypeBuilder<Session> builder)
  {
    builder
      .HasMany(x => x.AuthorizationCodeGrants)
      .WithOne(x => x.Session)
      .OnDelete(DeleteBehavior.Cascade);
  }
}