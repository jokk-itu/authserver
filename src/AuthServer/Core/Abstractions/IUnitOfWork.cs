namespace AuthServer.Core.Abstractions;

internal interface IUnitOfWork : IDisposable
{
    IDisposable Begin();
    Task SaveChanges();
    Task Commit();
}