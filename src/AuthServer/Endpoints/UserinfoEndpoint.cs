using System.Text;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Userinfo;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Endpoints;
internal class UserinfoEndpoint
{
    public static async Task<IResult> HandleUserinfo(
        HttpContext httpContext,
        IRequestAccessor<UserinfoRequest> requestAccessor,
        IRequestHandler<UserinfoRequest, string> requestHandler,
        CancellationToken cancellationToken)
    {
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var response = await requestHandler.Handle(request, cancellationToken);
        return response.Match(
            userinfo =>
            {
                var contentType = TokenHelper.IsStructuredToken(userinfo) ? MimeTypeConstants.Jwt : MimeTypeConstants.Json;
                return Results.Text(userinfo, contentType, Encoding.UTF8, StatusCodes.Status200OK);
            },
            error => error.ResultCode switch
            {
                ResultCode.Unauthorized => Results.Unauthorized(),
                _ => Results.Extensions.OAuthBadRequest(error)
            });
    }
}