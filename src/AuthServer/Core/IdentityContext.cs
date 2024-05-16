using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Core;
internal sealed class IdentityContext(DbContextOptions<IdentityContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}