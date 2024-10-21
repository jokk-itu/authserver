namespace AuthServer.Core.Abstractions;

internal interface IUnitOfWork
{
    Task Begin();
    Task SaveChanges();
    Task Commit();
}