using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class AuthenticationContextReferenceConfiguration : IEntityTypeConfiguration<AuthenticationContextReference>
{
    public void Configure(EntityTypeBuilder<AuthenticationContextReference> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasIndex(x => x.Name)
            .IsUnique();
    }
}
