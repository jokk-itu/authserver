namespace AuthServer.Core.Abstractions;

internal interface IUnitOfWork
{
    Task Begin();
    Task SaveChanges(CancellationToken cancellationToken);
    Task Commit(CancellationToken cancellationToken);
}