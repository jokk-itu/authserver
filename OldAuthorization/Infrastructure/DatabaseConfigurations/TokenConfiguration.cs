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
      .HasValue<GrantAccessToken>(TokenType.GrantAccessToken)
      .HasValue<ClientAccessToken>(TokenType.ClientAccessToken)
      .HasValue<RefreshToken>(TokenType.RefreshToken)
      .HasValue<RegistrationToken>(TokenType.RegistrationToken);

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