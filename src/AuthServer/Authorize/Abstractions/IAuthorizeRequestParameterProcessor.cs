using AuthServer.RequestAccessors.Authorize;

namespace AuthServer.Authorize.Abstractions;
internal interface IAuthorizeRequestParameterProcessor
{
    Task<AuthorizeRequest?> GetRequestByObject(string requestObject, string clientId, CancellationToken cancellationToken);
    Task<AuthorizeRequest?> GetRequestByReference(Uri requestUri, string clientId, CancellationToken cancellationToken);
    AuthorizeRequest GetCachedRequest();
}
