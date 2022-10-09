using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class AuthorizationCodeGrantConfiguration : IEntityTypeConfiguration<AuthorizationCodeGrant>
{
  public void Configure(EntityTypeBuilder<AuthorizationCodeGrant> builder)
  {
    builder.ToTable("AuthorizationCodeGrants");
  }
}