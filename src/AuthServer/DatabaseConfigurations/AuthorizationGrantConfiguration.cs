using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class AuthorizationGrantConfiguration : IEntityTypeConfiguration<AuthorizationGrant>
{
    public void Configure(EntityTypeBuilder<AuthorizationGrant> builder)
    {
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
            .HasOne(x => x.SubjectIdentifier)
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
            .WithMany(x => x.AuthorizationGrants);
    }
}