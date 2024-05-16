using AuthServer.Constants;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
    private sealed record ScopeSeed(int Id, string Name);

    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasIndex(s => s.Name)
            .IsUnique();

        builder.HasData(
            [
                new ScopeSeed(1, ScopeConstants.OpenId),
                new ScopeSeed(2, ScopeConstants.OfflineAccess),
                new ScopeSeed(3, ScopeConstants.UserInfo),
                new ScopeSeed(4, ScopeConstants.ClientConfiguration)
            ]);
    }
}