namespace AuthServer.Core.Exceptions;

public class ClientJwkRefreshException : Exception
{
    public ClientJwkRefreshException()
    {
    }

    public ClientJwkRefreshException(string message)
        : base(message)
    {
    }

    public ClientJwkRefreshException(string message, Exception inner)
        : base(message, inner)
    {
    }
}