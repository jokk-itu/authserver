using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class GrantTypeConfiguration : IEntityTypeConfiguration<Domain.GrantType>
{
  public void Configure(EntityTypeBuilder<Domain.GrantType> builder)
  {
    builder
      .Property(grant => grant.Name)
      .IsRequired();

    builder.HasData(
      new Domain.GrantType 
      {
        Id = 1,
        Name = GrantTypeConstants.AuthorizationCode
      },
      new Domain.GrantType 
      {
        Id = 2,
        Name = GrantTypeConstants.RefreshToken
      },
      new Domain.GrantType
      {
        Id = 3,
        Name = GrantTypeConstants.ClientCredentials
      });
  }
}
