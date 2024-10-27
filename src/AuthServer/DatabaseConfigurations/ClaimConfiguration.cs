using AuthServer.Constants;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    private sealed record ClaimSeed(int Id, string Name);

    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasIndex(x => x.Name)
            .IsUnique();

        builder
            .HasData(
            [
                new ClaimSeed(1, ClaimNameConstants.Name),
                new ClaimSeed(2, ClaimNameConstants.GivenName),
                new ClaimSeed(3, ClaimNameConstants.FamilyName),
                new ClaimSeed(4, ClaimNameConstants.MiddleName),
                new ClaimSeed(5, ClaimNameConstants.NickName),
                new ClaimSeed(6, ClaimNameConstants.PreferredUsername),
                new ClaimSeed(7, ClaimNameConstants.Profile),
                new ClaimSeed(8, ClaimNameConstants.Picture),
                new ClaimSeed(9, ClaimNameConstants.Website),
                new ClaimSeed(10, ClaimNameConstants.Email),
                new ClaimSeed(11, ClaimNameConstants.EmailVerified),
                new ClaimSeed(12, ClaimNameConstants.Gender),
                new ClaimSeed(13, ClaimNameConstants.Birthdate),
                new ClaimSeed(14, ClaimNameConstants.ZoneInfo),
                new ClaimSeed(15, ClaimNameConstants.Locale),
                new ClaimSeed(16, ClaimNameConstants.PhoneNumber),
                new ClaimSeed(17, ClaimNameConstants.PhoneNumberVerified),
                new ClaimSeed(18, ClaimNameConstants.Address),
                new ClaimSeed(19, ClaimNameConstants.UpdatedAt),
                new ClaimSeed(20, ClaimNameConstants.Roles),
            ]);
    }
}