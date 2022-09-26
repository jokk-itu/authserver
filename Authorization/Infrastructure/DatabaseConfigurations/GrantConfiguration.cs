using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class GrantConfiguration : IEntityTypeConfiguration<Grant>
{
  public void Configure(EntityTypeBuilder<Grant> builder)
  {
    builder
      .Property(grant => grant.Name)
      .HasConversion<string>();

    builder.HasData(
      new Grant 
      {
        Id = 1,
        Name = GrantType.AuthorizationCode
      }, 
      new Grant 
      {
        Id = 2,
        Name = GrantType.RefreshToken
      });

    builder.ToTable("Grants");
  }
}
