using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class SectorIdentifierConfiguration : IEntityTypeConfiguration<SectorIdentifier>
{
    public void Configure(EntityTypeBuilder<SectorIdentifier> builder)
    {
        builder
            .Property(x => x.Uri)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .Property(x => x.Salt)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasIndex(x => x.Uri)
            .IsUnique();
    }
}
