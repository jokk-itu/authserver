using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;
internal sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder
            .Property(x => x.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasOne(x => x.Client)
            .WithMany(x => x.Contacts)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}