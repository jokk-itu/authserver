using AuthServer.Constants;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class GrantTypeConfiguration : IEntityTypeConfiguration<GrantType>
{
    private sealed record GrantTypeSeed(int Id, string Name);

    public void Configure(EntityTypeBuilder<GrantType> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
        [
            new GrantTypeSeed(1, GrantTypeConstants.AuthorizationCode),
            new GrantTypeSeed(2, GrantTypeConstants.ClientCredentials),
            new GrantTypeSeed(3, GrantTypeConstants.RefreshToken)
        ]);
    }
}