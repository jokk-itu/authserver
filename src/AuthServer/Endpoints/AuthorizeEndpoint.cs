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
        CancellationToken cancellationToken)
    {
        // TODO find a way to get the UserId securely from the Interaction pages (login, consent and select_account)
        // Just get the UserId from the HttpContext.User property

        var options = userInteractionOptions.Value;
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var response = await requestHandler.Handle(request, cancellationToken);
        return await response.Match(
            async code => await authorizeResponseBuilder.BuildResponse(request, new Dictionary<string, string>{ { Parameter.Code, code } }, cancellationToken),
            async error => string.IsNullOrEmpty(request.Prompt)
                ? error switch
                {
                    { ResultCode: ResultCode.BadRequest } => Results.Extensions.OAuthBadRequest(error),
                    { Error: ErrorCode.LoginRequired } => Results.Redirect(options.LoginUri),
                    { Error: ErrorCode.ConsentRequired } => Results.Redirect(options.ConsentUri),
                    { Error: ErrorCode.AccountSelectionRequired } => Results.Redirect(options.AccountSelectionUri),
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
}