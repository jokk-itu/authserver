using AuthServer.Constants;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class ResponseTypeConfiguration : IEntityTypeConfiguration<ResponseType>
{
    private sealed record ResponseTypeSeed(int Id, string Name);

    public void Configure(EntityTypeBuilder<ResponseType> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
        [
            new ResponseTypeSeed(1, ResponseTypeConstants.Code)
        ]);
    }
}