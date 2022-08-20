using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class GrantConfiguration : IEntityTypeConfiguration<Grant>
{
  public void Configure(EntityTypeBuilder<Grant> builder)
  {
    builder.HasData(
      new Grant 
      {
        Id = 1,
        Name = GrantConstants.AuthorizationCode
      }, 
      new Grant 
      {
        Id = 2,
        Name = GrantConstants.RefreshToken
      });

    builder.ToTable("ClientGrants");
  }
}
