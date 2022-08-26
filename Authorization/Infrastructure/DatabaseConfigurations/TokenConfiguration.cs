using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class TokenConfiguration : IEntityTypeConfiguration<Token>
{
  public void Configure(EntityTypeBuilder<Token> builder)
  {
    builder.HasKey(token => token.KeyId);
    builder
      .Property(token => token.TokenType)
      .HasConversion<string>();

    builder
      .Property(token => token.Value)
      .HasMaxLength(int.MaxValue);

    builder.HasOne(token => token.RevokedBy);
    builder.ToTable("Tokens");
  }
}
