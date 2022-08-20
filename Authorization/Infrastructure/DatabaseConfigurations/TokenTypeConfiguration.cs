using Domain;
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

    builder.ToTable("TokenTypes");
  }
}
