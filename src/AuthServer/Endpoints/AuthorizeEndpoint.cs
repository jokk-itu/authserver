using AuthServer.Authorize.Abstractions;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Authorize;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthServer.Endpoints;

internal static class AuthorizeEndpoint
{
    public static async Task<IResult> HandleAuthorize(
        HttpContext httpContext,
        [FromServices] IRequestAccessor<AuthorizeRequest> requestAccessor,
        [FromServices] IRequestHandler<AuthorizeRequest, string> requestHandler,
        [FromServices] IAuthorizeResponseBuilder authorizeResponseBuilder,
        [FromServices] IOptionsSnapshot<UserInteraction> userInteractionOptions,
        [FromServices] IUserAccessor userAccessor,
        [FromServices] IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        CancellationToken cancellationToken)
    {
        var options = userInteractionOptions.Value;
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var response = await requestHandler.Handle(request, cancellationToken);

        return await response.Match(
            async code =>
            {
                userAccessor.ClearUser();
                return await authorizeResponseBuilder.BuildResponse(request,
                    new Dictionary<string, string> { { Parameter.Code, code } }, cancellationToken);
            },
            async error => string.IsNullOrEmpty(request.Prompt)
                ? error switch
                {
                    { ResultCode: ResultCode.BadRequest } => Results.Extensions.OAuthBadRequest(error),
                    { Error: ErrorCode.LoginRequired } => Redirect(options.LoginUri, httpContext, discoveryDocumentOptions.Value),
                    { Error: ErrorCode.ConsentRequired } => Results.Extensions.OAuthSeeOtherRedirect(options.ConsentUri, httpContext.Response),
                    { Error: ErrorCode.AccountSelectionRequired } => Results.Extensions.OAuthSeeOtherRedirect(options.AccountSelectionUri, httpContext.Response),
                    { ResultCode: ResultCode.Redirect} => await authorizeResponseBuilder.BuildResponse(request, error.ToDictionary(), cancellationToken),
                    _ => Results.Extensions.OAuthBadRequest(error)
                }
                : error switch
                {
                    { ResultCode: ResultCode.BadRequest } => Results.Extensions.OAuthBadRequest(error),
                    { ResultCode: ResultCode.Redirect } => Results.Ok(),
                    _ => Results.Extensions.OAuthBadRequest(error)
                });
    }

    private static IResult Redirect(string url, HttpContext httpContext, DiscoveryDocument discoveryDocument)
    {
        var returnUrl = httpContext.Request.GetEncodedUrl();
        var location = $"{url}?returnUrl={returnUrl}";
        return Results.Extensions.OAuthSeeOtherRedirect(location, httpContext.Response);
    }
}