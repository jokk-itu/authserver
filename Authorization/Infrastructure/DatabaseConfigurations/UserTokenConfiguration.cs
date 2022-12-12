using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
  public void Configure(EntityTypeBuilder<UserToken> builder)
  {
   builder.ToTable("UserTokens");
  }
}
