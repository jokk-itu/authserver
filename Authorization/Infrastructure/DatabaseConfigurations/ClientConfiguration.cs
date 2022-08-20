using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ClientConfiguration : IEntityTypeConfiguration<Client>
{
  public void Configure(EntityTypeBuilder<Client> builder)
  {
    builder.HasOne(client => client.ClientType);
    builder.HasOne(client => client.ClientProfile);
    builder.HasMany(client => client.RedirectUris);

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
