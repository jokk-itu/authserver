﻿using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class ConsentGrantConfiguration : IEntityTypeConfiguration<ConsentGrant>
{
  public void Configure(EntityTypeBuilder<ConsentGrant> builder)
  {
    builder.HasBaseType<Grant>();
    builder.HasOne(x => x.Client);
    builder.HasOne(x => x.User);
    builder
      .HasMany(x => x.ConsentedClaims)
      .WithMany(x => x.ConsentGrants)
      .UsingEntity(x => x.ToTable("ConsentedGrantClaims"));
    builder.ToTable("ConsentGrants");
  }
}