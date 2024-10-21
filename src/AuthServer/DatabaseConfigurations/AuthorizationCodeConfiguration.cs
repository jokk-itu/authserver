using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class AuthorizationCodeConfiguration : IEntityTypeConfiguration<AuthorizationCode>
{
    public void Configure(EntityTypeBuilder<AuthorizationCode> builder)
    {
        builder
            .Property(x => x.Value)
            .HasMaxLength(2048)
            .IsRequired();

        builder
            .HasOne(x => x.AuthorizationGrant)
            .WithMany(x => x.AuthorizationCodes)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}