using AuthServer.RequestAccessors.Authorize;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Authorize.Abstractions;

internal interface IAuthorizeResponseBuilder
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="additionalParameters"></param>
    /// <param name="httpResponse"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IResult> BuildResponse(AuthorizeRequest request, IDictionary<string, string> additionalParameters,
        HttpResponse httpResponse, CancellationToken cancellationToken);
}