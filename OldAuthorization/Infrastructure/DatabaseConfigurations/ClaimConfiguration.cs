using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
  public void Configure(EntityTypeBuilder<Claim> builder)
  {
    builder
      .Property(x => x.Name)
      .HasMaxLength(32);

    builder.HasData(
      new Claim
      {
        Id = 1,
        Name = ClaimNameConstants.Name
      }, new Claim
      {
        Id = 2,
        Name = ClaimNameConstants.GivenName
      }, new Claim
      {
        Id = 3,
        Name = ClaimNameConstants.FamilyName
      }, new Claim
      {
        Id = 4,
        Name = ClaimNameConstants.Phone
      }, new Claim
      {
        Id = 5,
        Name = ClaimNameConstants.Email
      }, new Claim
      {
        Id = 6,
        Name = ClaimNameConstants.Address
      }, new Claim
      {
        Id = 7,
        Name = ClaimNameConstants.Birthdate
      }, new Claim
      {
        Id = 8,
        Name = ClaimNameConstants.Locale
      }, new Claim
      {
        Id = 9,
        Name = ClaimNameConstants.Role
      });
  }
}
