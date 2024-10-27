using Microsoft.AspNetCore.Http;

namespace AuthServer.Core.Abstractions;
public interface IRequestAccessor<TRequest>
    where TRequest : class
{
    Task<TRequest> GetRequest(HttpRequest httpRequest);
}