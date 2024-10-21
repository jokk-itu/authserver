using AuthServer.Core.Abstractions;

namespace AuthServer.Core;

internal class UnitOfWork : IUnitOfWork
{
    private readonly AuthorizationDbContext _authorizationDbContext;

    public UnitOfWork(AuthorizationDbContext authorizationDbContext)
    {
        _authorizationDbContext = authorizationDbContext;
    }

    public async Task Begin()
    {
        if (_authorizationDbContext.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException();
        }

        await _authorizationDbContext.Database.BeginTransactionAsync();
    }

    public async Task SaveChanges()
    {
        if (_authorizationDbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException();
        }

        await _authorizationDbContext.SaveChangesAsync();
    }

    public async Task Commit()
    {
        if (_authorizationDbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException();
        }

        await _authorizationDbContext.Database.CommitTransactionAsync();
    }
}