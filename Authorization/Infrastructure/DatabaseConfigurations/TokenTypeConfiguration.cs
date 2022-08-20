using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class TokenTypeConfiguration : IEntityTypeConfiguration<TokenType>
{
  public void Configure(EntityTypeBuilder<TokenType> builder)
  {
    builder
      .HasIndex(tokenType => tokenType.Name)
      .IsUnique(true);

    builder.HasData(
      new TokenType
      {
        Id = 1,
        Name = TokenTypeConstants.RefreshToken
      },
      new TokenType
      {
        Id = 2,
        Name = TokenTypeConstants.AccessToken
      });

    builder.ToTable("TokenTypes");
  }
}
