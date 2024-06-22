using AuthServer.Entities;

namespace AuthServer.Cache.Abstractions;

internal interface IEntityInMemoryCache
{
    Task<Scope> GetScope(string scope, CancellationToken cancellationToken);
    Task<GrantType> GetGrantType(string grantType, CancellationToken cancellationToken);
    Task<ResponseType> GetResponseType(string responseType, CancellationToken cancellationToken);
}