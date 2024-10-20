using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class AuthorizationGrantConfiguration : IEntityTypeConfiguration<AuthorizationGrant>
{
    public void Configure(EntityTypeBuilder<AuthorizationGrant> builder)
    {
        builder
            .Property(x => x.Subject)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.AuthorizationGrants)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasOne(x => x.Session)
            .WithMany(x => x.AuthorizationGrants)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasOne(x => x.AuthenticationContextReference)
            .WithMany(x => x.AuthorizationGrants)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasMany(x => x.AuthenticationMethodReferences)
            .WithMany(x => x.AuthorizationGrants)
            .UsingEntity(
                $"{nameof(AuthorizationGrant)}{nameof(AuthenticationMethodReference)}",
                r => r.HasOne(typeof(AuthenticationMethodReference)).WithMany().HasForeignKey($"{nameof(AuthenticationMethodReference)}Id"),
                l => l.HasOne(typeof(AuthorizationGrant)).WithMany().HasForeignKey($"{nameof(AuthorizationGrant)}Id"));
    }
}