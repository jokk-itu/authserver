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
      .Property(x => x.Name)
      .IsRequired();

    builder
      .Property(client => client.ApplicationType)
      .HasConversion<string>();

    builder
      .Property(client => client.SubjectType)
      .HasConversion<string>();

    builder
      .Property(client => client.TokenEndpointAuthMethod)
      .HasConversion<string>();

    builder
      .HasMany(client => client.RedirectUris)
      .WithOne(redirectUri => redirectUri.Client)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasMany(client => client.Scopes)
      .WithMany(scope => scope.Clients);

    builder
      .HasMany(client => client.GrantTypes)
      .WithMany(grant => grant.Clients);

    builder
      .HasMany(client => client.Contacts)
      .WithMany(contact => contact.Clients);

    builder
      .HasMany(client => client.ResponseTypes)
      .WithMany(contact => contact.Clients);

    builder
      .HasMany(x => x.AuthorizationCodeGrants)
      .WithOne(x => x.Client)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasMany(x => x.ConsentGrants)
      .WithOne(x => x.Client)
      .OnDelete(DeleteBehavior.Cascade);
  }
}