using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
  public void Configure(EntityTypeBuilder<Scope> builder)
  {
    builder
      .HasIndex(s => s.Name)
      .IsUnique();

    builder.HasData(
      new Scope 
      {
        Id = 1,
        Name = ScopeConstants.OpenId
      },
      new Scope
      {
        Id = 2,
        Name = ScopeConstants.Email
      },
      new Scope
      {
        Id = 3,
        Name = ScopeConstants.Profile
      },
      new Scope
      {
        Id = 4,
        Name = ScopeConstants.OfflineAccess
      },
      new Scope
      {
        Id = 5,
        Name = ScopeConstants.Phone
      },
      new Scope
      {
        Id = 6,
        Name = ScopeConstants.UserInfo
      });
  }
}