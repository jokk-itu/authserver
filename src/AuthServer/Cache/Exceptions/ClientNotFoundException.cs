namespace AuthServer.Cache.Exceptions;
public class ClientNotFoundException : Exception
{
    public ClientNotFoundException(string clientId)
        : base($"Client {clientId} not found")
    {
    }

    public ClientNotFoundException(string clientId, Exception inner)
        : base($"Client {clientId} not found", inner)
    {
    }
}