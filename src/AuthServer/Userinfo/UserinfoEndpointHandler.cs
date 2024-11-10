using System.Text;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Userinfo;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Userinfo;
internal class UserinfoEndpointHandler : IEndpointHandler
{
    private readonly IRequestAccessor<UserinfoRequest> _requestAccessor;
    private readonly IRequestHandler<UserinfoRequest, string> _requestHandler;

    public UserinfoEndpointHandler(
        IRequestAccessor<UserinfoRequest> requestAccessor,
        IRequestHandler<UserinfoRequest, string> requestHandler)
    {
        _requestAccessor = requestAccessor;
        _requestHandler = requestHandler;
    }

    public async Task<IResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var request = await _requestAccessor.GetRequest(httpContext.Request);
        var response = await _requestHandler.Handle(request, cancellationToken);
        return response.Match(
            userinfo =>
            {
                var contentType = TokenHelper.IsJsonWebToken(userinfo) ? MimeTypeConstants.Jwt : MimeTypeConstants.Json;
                return Results.Text(userinfo, contentType, Encoding.UTF8, StatusCodes.Status200OK);
            },
            error => error.ResultCode switch
            {
                ResultCode.Unauthorized => Results.Unauthorized(),
                _ => Results.Extensions.OAuthBadRequest(error)
            });
    }
}
