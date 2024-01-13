using Domain.Constants;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class GrantTypeConfiguration : IEntityTypeConfiguration<GrantType>
{
  public void Configure(EntityTypeBuilder<GrantType> builder)
  {
    builder
      .Property(grant => grant.Name)
      .IsRequired();

    builder.HasData(
      new GrantType 
      {
        Id = 1,
        Name = GrantTypeConstants.AuthorizationCode
      },
      new GrantType 
      {
        Id = 2,
        Name = GrantTypeConstants.RefreshToken
      },
      new GrantType
      {
        Id = 3,
        Name = GrantTypeConstants.ClientCredentials
      });
  }
}
