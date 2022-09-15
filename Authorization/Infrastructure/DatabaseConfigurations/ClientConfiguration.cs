using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ClientConfiguration : IEntityTypeConfiguration<Client>
{
  public void Configure(EntityTypeBuilder<Client> builder)
  {
    builder
      .Property(client => client.ClientType)
      .HasConversion<string>();

    builder
      .Property(client => client.ClientProfile)
      .HasConversion<string>();

    builder.HasMany(client => client.RedirectUris);

    builder
      .HasMany(client => client.Scopes)
      .WithMany(scope => scope.Clients)
      .UsingEntity(link => link.ToTable("ClientScopes"));

    builder
      .HasMany(client => client.Grants);
      
    builder.ToTable("Clients");
  }
}