using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class AuthorizationCodeConfiguration : IEntityTypeConfiguration<AuthorizationCode>
{
  public void Configure(EntityTypeBuilder<AuthorizationCode> builder)
  {
    
  }
}
