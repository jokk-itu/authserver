using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.Property(x => x.UserName).IsRequired();
    builder.HasIndex(x => x.UserName).IsUnique();

    builder
      .HasMany(u => u.UserTokens)
      .WithOne(ut => ut.User)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(x => x.Password).IsRequired();
    builder.ToTable("Users");
  }
}