using AuthServer.Authorization;
using AuthServer.Entities;

namespace AuthServer.Repositories.Abstractions;
internal interface IClientRepository
{
    /// <summary>
    /// Returns whether resources map to existing scope.
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="scopes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> DoesResourcesExist(IReadOnlyCollection<string> resources, IReadOnlyCollection<string> scopes, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizeRequestDto?> GetAuthorizeDto(string reference, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="authorizeDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizeMessage> AddAuthorizeMessage(AuthorizeRequestDto authorizeDto, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RedeemAuthorizeMessage(string reference, CancellationToken cancellationToken);
}