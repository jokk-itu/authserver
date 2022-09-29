using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class AuthorizationGrantConfiguration : IEntityTypeConfiguration<AuthorizationGrant>
{
  public void Configure(EntityTypeBuilder<AuthorizationGrant> builder)
  {
    builder.HasBaseType<AuthorizationGrant>();
    builder.HasOne(x => x.Client);
    builder.HasOne(x => x.User);
    builder.ToTable("AuthorizationGrants");
  }
}