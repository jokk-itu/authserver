using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class RequestUriConfiguration : IEntityTypeConfiguration<RequestUri>
{
    public void Configure(EntityTypeBuilder<RequestUri> builder)
    {
        builder
            .Property(x => x.Uri)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.RequestUris)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}