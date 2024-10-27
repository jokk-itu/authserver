using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Core;
public sealed class AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}