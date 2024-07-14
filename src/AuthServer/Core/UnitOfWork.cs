using System.Transactions;
using AuthServer.Core.Abstractions;

namespace AuthServer.Core;

internal class UnitOfWork : IUnitOfWork
{
    private TransactionScope? _currentTransaction;
    private readonly AuthorizationDbContext _authorizationDbContext;

    public UnitOfWork(AuthorizationDbContext authorizationDbContext)
    {
        _authorizationDbContext = authorizationDbContext;
    }

    public IDisposable Begin()
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException();
        }

        _currentTransaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        return _currentTransaction;
    }

    public IDisposable Current() => _currentTransaction ?? throw new InvalidOperationException();

    public async Task SaveChanges()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException();
        }

        await _authorizationDbContext.SaveChangesAsync();
    }

    public async Task Commit()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException();
        }

        await _authorizationDbContext.SaveChangesAsync();
        _currentTransaction.Complete();
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
    }
}