using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Infrastructure.DatabaseConfigurations;
internal class GrantConfiguration : IEntityTypeConfiguration<Grant>
{
  public void Configure(EntityTypeBuilder<Grant> builder)
  {
    builder.HasData(
      new Grant 
      {
        Id = 1,
        Name = OpenIdConnectGrantTypes.AuthorizationCode
      }, 
      new Grant 
      {
        Id = 2,
        Name = OpenIdConnectGrantTypes.RefreshToken
      });

    builder.ToTable("Grants");
  }
}
