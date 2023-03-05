using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.Property(x => x.UserName).IsRequired();
    builder.Property(x => x.Password).IsRequired();

    builder.HasIndex(x => x.UserName).IsUnique();

    builder.ToTable("Users");
  }
}