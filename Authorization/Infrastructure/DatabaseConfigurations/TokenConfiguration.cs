using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;

#nullable disable
public class TokenConfiguration : IEntityTypeConfiguration<Token>
{
  public void Configure(EntityTypeBuilder<Token> builder)
  {
    builder
      .HasDiscriminator(x => x.TokenType)
      .HasValue(typeof(AccessToken), TokenType.AccessToken)
      .HasValue(typeof(IdToken), TokenType.IdToken)
      .HasValue(typeof(RefreshToken), TokenType.RefreshToken)
      .HasValue(typeof(ScopeRegistrationToken), TokenType.ScopeRegistrationToken)
      .HasValue(typeof(ClientRegistrationToken), TokenType.ClientRegistrationToken)
      .HasValue(typeof(ResourceRegistrationToken), TokenType.ResourceRegistrationToken)
      .HasValue(typeof(Token), TokenType.ClientInitialToken)
      .HasValue(typeof(Token), TokenType.ScopeInitialToken)
      .HasValue(typeof(Token), TokenType.ResourceInitialToken)
      .HasValue(TokenType.Token);

    builder.ToTable("Tokens");
  }
}
