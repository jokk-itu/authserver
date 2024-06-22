namespace AuthServer.Core.Abstractions;

internal interface IUnitOfWork : IDisposable
{
    IDisposable Begin();
    IDisposable Current();
    Task SaveChanges();
    Task Commit();
}