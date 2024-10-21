using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class AuthorizeMessageConfiguration : IEntityTypeConfiguration<AuthorizeMessage>
{
    public void Configure(EntityTypeBuilder<AuthorizeMessage> builder)
    {
        builder
            .Property(x => x.Value)
            .HasMaxLength(4096)
            .IsRequired();

        builder
            .Property(x => x.Reference)
            .HasMaxLength(256)
            .IsRequired();

        builder
            .HasIndex(x => x.Reference)
            .IsUnique();

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.AuthorizeMessages)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}
