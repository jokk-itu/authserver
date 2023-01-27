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
      .Property(client => client.ApplicationType)
      .HasConversion<string>();

    builder
      .Property(client => client.SubjectType)
      .HasConversion<string>();

    builder
      .Property(client => client.TokenEndpointAuthMethod)
      .HasConversion<string>();

    builder
      .Property(client => client.TosUri)
      .IsRequired(false);

    builder
      .Property(client => client.PolicyUri)
      .IsRequired(false);

    builder
      .HasMany(client => client.RedirectUris)
      .WithOne(redirectUri => redirectUri.Client)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasMany(client => client.Scopes)
      .WithMany(scope => scope.Clients)
      .UsingEntity(link => link.ToTable("ClientScopes"));

    builder
      .HasMany(client => client.GrantTypes)
      .WithMany(grant => grant.Clients)
      .UsingEntity(link => link.ToTable("ClientGrantTypes"));

    builder
      .HasMany(client => client.Contacts)
      .WithMany(contact => contact.Clients)
      .UsingEntity(link => link.ToTable("ClientContacts"));

    builder
      .HasMany(client => client.ResponseTypes)
      .WithMany(contact => contact.Clients)
      .UsingEntity(link => link.ToTable("ClientResponseTypes"));

    builder
      .HasMany(x => x.AuthorizationCodeGrants)
      .WithOne(x => x.Client)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasMany(x => x.ConsentGrants)
      .WithOne(x => x.Client)
      .OnDelete(DeleteBehavior.Cascade);

    builder.ToTable("Clients");
  }
}