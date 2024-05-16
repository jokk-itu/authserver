using AuthServer.Entities;
using AuthServer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class TokenConfiguration : IEntityTypeConfiguration<Token>
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
            .HasMaxLength(255)
            .IsRequired();

        builder
            .Property(x => x.Issuer)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .Property(x => x.Reference)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .Property(x => x.Scope)
            .HasMaxLength(255);

        builder
            .HasIndex(x => x.Reference)
            .IsUnique();
    }
}