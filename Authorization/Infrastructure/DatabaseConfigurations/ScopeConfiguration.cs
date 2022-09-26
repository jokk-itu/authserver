﻿using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Infrastructure.DatabaseConfigurations;
internal class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
  public void Configure(EntityTypeBuilder<Scope> builder)
  {
    builder
      .HasIndex(s => s.Name)
      .IsUnique(true);

    builder.HasData(
      new Scope 
      {
        Id = 1,
        Name = OpenIdConnectScope.OpenId
      },
      new Scope
      {
        Id = 2,
        Name = OpenIdConnectScope.Email
      },
      new Scope
      {
        Id = 3,
        Name = ScopeConstants.Profile
      },
      new Scope
      {
        Id = 4,
        Name = OpenIdConnectScope.OfflineAccess
      },
      new Scope
      {
        Id = 5,
        Name = ScopeConstants.Phone
      });

    builder.ToTable("Scopes");
  }
}
