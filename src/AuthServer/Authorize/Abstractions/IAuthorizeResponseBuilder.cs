using AuthServer.RequestAccessors.Authorize;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Authorize.Abstract;

internal interface IAuthorizeResponseBuilder
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="additionalParameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IResult> BuildResponse(AuthorizeRequest request, IDictionary<string, string> additionalParameters,
        CancellationToken cancellationToken);
}