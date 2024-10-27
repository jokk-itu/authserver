using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
  public void Configure(EntityTypeBuilder<Session> builder)
  {
      builder
          .HasOne(x => x.SubjectIdentifier)
          .WithMany(x => x.Sessions)
          .IsRequired()
          .OnDelete(DeleteBehavior.ClientCascade);
  }
}