using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder
      .Property(x => x.UserName)
      .IsRequired();

    builder
      .Property(x => x.Password)
      .IsRequired();

    builder
      .Property(x => x.PhoneNumber)
      .IsRequired();

    builder.Property(x => x.Email)
      .IsRequired();

    builder
      .Property(x => x.Address)
      .IsRequired();

    builder
      .Property(x => x.LastName)
      .IsRequired();

    builder
      .Property(x => x.FirstName)
      .IsRequired();

    builder
      .Property(x => x.Locale)
      .IsRequired();

    builder
      .HasMany(x => x.Sessions)
      .WithOne(x => x.User)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasMany(x => x.Roles)
      .WithMany(x => x.Users);

    builder
      .HasIndex(x => x.UserName)
      .IsUnique();

    builder
      .HasIndex(x => x.Email)
      .IsUnique();

    builder
      .HasIndex(x => x.PhoneNumber)
      .IsUnique();
  }
}