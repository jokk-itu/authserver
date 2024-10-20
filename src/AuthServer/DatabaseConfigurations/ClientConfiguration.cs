using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .Property(x => x.SecretHash)
            .HasMaxLength(255);

        builder
            .Property(x => x.TosUri)
            .HasMaxLength(255);

        builder
            .Property(x => x.PolicyUri)
            .HasMaxLength(255);

        builder
            .Property(x => x.ClientUri)
            .HasMaxLength(255);

        builder
            .Property(x => x.LogoUri)
            .HasMaxLength(255);

        builder
            .Property(x => x.InitiateLoginUri)
            .HasMaxLength(255);

        builder
            .Property(x => x.BackchannelLogoutUri)
            .HasMaxLength(255);

        builder
            .Property(x => x.JwksUri)
            .HasMaxLength(255);

        builder
            .Property(x => x.Jwks)
            .HasMaxLength(int.MaxValue);

        builder
            .Property(client => client.ApplicationType)
            .HasConversion<int>();

        builder
            .Property(client => client.SubjectType)
            .HasConversion<int>();

        builder
            .Property(client => client.TokenEndpointAuthMethod)
            .HasConversion<int>();

        builder
            .HasMany(client => client.Scopes)
            .WithMany(scope => scope.Clients)
            .UsingEntity(
                $"{nameof(Client)}{nameof(Scope)}",
                r => r.HasOne(typeof(Scope)).WithMany().HasForeignKey($"{nameof(Scope)}Id"),
                l => l.HasOne(typeof(Client)).WithMany().HasForeignKey($"{nameof(Client)}Id"));

        builder
            .HasMany(client => client.GrantTypes)
            .WithMany(grant => grant.Clients)
            .UsingEntity(
                $"{nameof(Client)}{nameof(GrantType)}",
                r => r.HasOne(typeof(GrantType)).WithMany().HasForeignKey($"{nameof(GrantType)}Id"),
                l => l.HasOne(typeof(Client)).WithMany().HasForeignKey($"{nameof(Client)}Id"));

        builder
            .HasMany(client => client.ResponseTypes)
            .WithMany(contact => contact.Clients)
            .UsingEntity(
                $"{nameof(Client)}{nameof(ResponseType)}",
                r => r.HasOne(typeof(ResponseType)).WithMany().HasForeignKey($"{nameof(ResponseType)}Id"),
                l => l.HasOne(typeof(Client)).WithMany().HasForeignKey($"{nameof(Client)}Id"));

        builder
            .HasMany(x => x.ClientAuthenticationContextReferences)
            .WithOne(x => x.Client);

        builder
            .HasOne(x => x.SectorIdentifier)
            .WithMany(x => x.Clients)
            .IsRequired(false);
    }
}