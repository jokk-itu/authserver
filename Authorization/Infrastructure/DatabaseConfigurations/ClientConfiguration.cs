using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ClientConfiguration : IEntityTypeConfiguration<Client>
{
  public void Configure(EntityTypeBuilder<Client> builder)
  {
    builder.HasKey(client => client.Id);

    builder
      .Property(client => client.ClientType)
      .HasConversion<string>();

    builder
      .Property(client => client.ClientProfile)
      .HasConversion<string>();

    builder
      .HasMany(client => client.RedirectUris)
      .WithOne(redirectUri => redirectUri.Client);

    builder
      .HasMany(client => client.Scopes)
      .WithMany(scope => scope.Clients)
      .UsingEntity(link => link.ToTable("ClientScopes"));

    builder
      .HasMany(client => client.Grants)
      .WithMany(grant => grant.Clients)
      .UsingEntity(link => link.ToTable("ClientGrants"));
      
    builder.ToTable("Clients");
  }
}