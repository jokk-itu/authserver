using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class ClientAuthenticationContextReferenceConfiguration : IEntityTypeConfiguration<ClientAuthenticationContextReference>
{
    public void Configure(EntityTypeBuilder<ClientAuthenticationContextReference> builder)
    {
        builder.HasKey(x => new { x.ClientId, x.AuthenticationContextReferenceId });

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.ClientAuthenticationContextReferences)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasOne(x => x.AuthenticationContextReference)
            .WithMany(x => x.ClientAuthenticationContextReferences)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}
