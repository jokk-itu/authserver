using AuthServer.Cache.Abstractions;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Core;

internal class UnitOfWork : IUnitOfWork
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly ICachedClientStore _cachedClientStore;

    public UnitOfWork(
        AuthorizationDbContext authorizationDbContext,
        ICachedClientStore cachedClientStore)
    {
        _authorizationDbContext = authorizationDbContext;
        _cachedClientStore = cachedClientStore;
    }

    public async Task Begin()
    {
        if (_authorizationDbContext.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException();
        }

        await _authorizationDbContext.Database.BeginTransactionAsync();
    }

    public async Task SaveChanges(CancellationToken cancellationToken)
    {
        if (_authorizationDbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException();
        }

        await _authorizationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Commit(CancellationToken cancellationToken)
    {
        await SaveChanges(cancellationToken);
        await _authorizationDbContext.Database.CommitTransactionAsync(cancellationToken);

        var clients = _authorizationDbContext.ChangeTracker
            .Entries<Client>()
            .Where(x => x.State == EntityState.Modified)
            .ToList();

        foreach (var client in clients)
        {
            await _cachedClientStore.Delete(client.Entity.Id, cancellationToken);
        }
    }
}