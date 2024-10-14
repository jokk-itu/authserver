using AuthServer.Authorize.Abstractions;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Authorize;
using Microsoft.AspNetCore.Http;
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
        [FromServices] IAuthorizeUserAccessor userAccessor,
        CancellationToken cancellationToken)
    {
        var options = userInteractionOptions.Value;
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var response = await requestHandler.Handle(request, cancellationToken);

        return await response.Match(
            async code =>
            {
                // remove the authorized user to reset the interaction flow
                userAccessor.ClearUser();
                return await authorizeResponseBuilder.BuildResponse(request,
                    new Dictionary<string, string> { { Parameter.Code, code } }, httpContext.Response, cancellationToken);
            },
            async error => string.IsNullOrEmpty(request.Prompt) && error.Error is ErrorCode.LoginRequired or ErrorCode.ConsentRequired or ErrorCode.AccountSelectionRequired
                ? error switch
                {
                    { Error: ErrorCode.LoginRequired } => Results.Extensions.LocalRedirect(options.LoginUri, httpContext),
                    { Error: ErrorCode.ConsentRequired } => Results.Extensions.LocalRedirect(options.ConsentUri, httpContext),
                    _ => Results.Extensions.LocalRedirect(options.AccountSelectionUri, httpContext)
                }
                : error switch
                {
                    { ResultCode: ResultCode.Redirect } => await authorizeResponseBuilder.BuildResponse(request, error.ToDictionary(), httpContext.Response, cancellationToken),
                    _ => Results.Extensions.OAuthBadRequest(error)
                });
    }
}