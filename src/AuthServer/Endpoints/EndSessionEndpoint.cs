using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Core;
using AuthServer.EndSession.Abstractions;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.EndSession;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthServer.Endpoints;

internal static class EndSessionEndpoint
{
    public static async Task<IResult> HandleEndSession(
        HttpContext httpContext,
        [FromServices] IRequestAccessor<EndSessionRequest> requestAccessor,
        [FromServices] IRequestHandler<EndSessionRequest, Unit> requestHandler,
        [FromServices] IOptionsSnapshot<UserInteraction> userInteractionOptions,
        [FromServices] IEndSessionUserAccessor userAccessor,
        CancellationToken cancellationToken)
    {
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var response = await requestHandler.Handle(request, cancellationToken);
        userAccessor.ClearUser();
        return response.Match(
            _ =>
            {
                if (string.IsNullOrEmpty(request.PostLogoutRedirectUri))
                {
                    return Results.Ok();
                }

                return Results.Extensions.OAuthSeeOtherRedirect($"{request.PostLogoutRedirectUri}?state={request.State}", httpContext.Response);
            },
            error => error switch
            {
                { Error: ErrorCode.InteractionRequired } => Results.Extensions.LocalRedirect(userInteractionOptions.Value.EndSessionUri, httpContext),
                _ => Results.Extensions.OAuthBadRequest(error)
            });
    }
}
