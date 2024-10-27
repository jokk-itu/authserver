using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class ClientTokenConfiguration : IEntityTypeConfiguration<ClientToken>
{
    public void Configure(EntityTypeBuilder<ClientToken> builder)
    {
        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.ClientTokens)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}