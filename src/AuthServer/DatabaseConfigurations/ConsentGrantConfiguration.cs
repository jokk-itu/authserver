using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class ConsentGrantConfiguration : IEntityTypeConfiguration<ConsentGrant>
{
    public void Configure(EntityTypeBuilder<ConsentGrant> builder)
    {
        builder
            .HasMany(x => x.ConsentedClaims)
            .WithMany(x => x.ConsentGrants)
            .UsingEntity(
                $"{nameof(ConsentGrant)}{nameof(Claim)}",
                r => r.HasOne(typeof(Claim)).WithMany().HasForeignKey($"{nameof(Claim)}Id"),
                l => l.HasOne(typeof(ConsentGrant)).WithMany().HasForeignKey($"{nameof(ConsentGrant)}Id"));

        builder
            .HasMany(x => x.ConsentedScopes)
            .WithMany(x => x.ConsentGrants)
            .UsingEntity(
                $"{nameof(ConsentGrant)}{nameof(Scope)}",
                r => r.HasOne(typeof(Scope)).WithMany().HasForeignKey($"{nameof(Scope)}Id"),
                l => l.HasOne(typeof(ConsentGrant)).WithMany().HasForeignKey($"{nameof(ConsentGrant)}Id"));

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.ConsentGrants)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasOne(x => x.SubjectIdentifier)
            .WithMany(x => x.ConsentGrants)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}