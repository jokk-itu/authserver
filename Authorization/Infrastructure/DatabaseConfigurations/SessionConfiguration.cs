﻿using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
  public void Configure(EntityTypeBuilder<Session> builder)
  {
    builder
      .HasMany(x => x.AuthorizationCodeGrants)
      .WithOne(x => x.Session);

    builder
      .HasMany(x => x.Clients)
      .WithMany(x => x.Sessions)
      .UsingEntity(x => x.ToTable("SessionClients"));

    builder
      .HasMany(x => x.IdTokens)
      .WithOne(x => x.Session);

    builder
      .HasMany(x => x.AccessTokens)
      .WithOne(x => x.Session);

    builder
      .HasMany(x => x.RefreshTokens)
      .WithOne(x => x.Session);

    builder
      .HasOne(x => x.User)
      .WithOne(x => x.Session);

    builder.ToTable("Sessions");
  }
}