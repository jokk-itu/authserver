using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class TokenConfiguration : IEntityTypeConfiguration<Token>
{
  public void Configure(EntityTypeBuilder<Token> builder)
  {
    builder
      .HasDiscriminator(x => x.TokenType)
      .HasValue<AccessToken>(TokenType.AccessToken)
      .HasValue<RefreshToken>(TokenType.RefreshToken);

    builder
      .Property(x => x.Scope)
      .IsRequired();

    builder
      .Property(x => x.Audience)
      .IsRequired();

    builder
      .Property(x => x.Issuer)
      .IsRequired();

    builder
      .Property(x => x.Reference)
      .IsRequired();
  }
}