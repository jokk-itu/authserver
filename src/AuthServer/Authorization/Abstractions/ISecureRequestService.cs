using AuthServer.TokenDecoders;

namespace AuthServer.Authorization.Abstractions;
internal interface ISecureRequestService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestObject"></param>
    /// <param name="clientId"></param>
    /// <param name="audience"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizeRequestDto?> GetRequestByObject(string requestObject, string clientId, ClientTokenAudience audience, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestUri"></param>
    /// <param name="clientId"></param>
    /// <param name="audience"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizeRequestDto?> GetRequestByReference(Uri requestUri, string clientId, ClientTokenAudience audience, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<AuthorizeRequestDto?> GetRequestByPushedRequest(string requestUri, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    AuthorizeRequestDto GetCachedRequest();
}
