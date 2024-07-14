using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class NonceConfiguration : IEntityTypeConfiguration<Nonce>
{
    public void Configure(EntityTypeBuilder<Nonce> builder)
    {
        builder
            .Property(x => x.Value)
            .HasMaxLength(int.MaxValue)
            .IsRequired();

        builder
            .HasOne(x => x.AuthorizationGrant)
            .WithMany(x => x.Nonces)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}