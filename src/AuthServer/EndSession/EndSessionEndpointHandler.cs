using System.Web;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.EndSession.Abstractions;
using AuthServer.Extensions;
using AuthServer.Options;
using AuthServer.RequestAccessors.EndSession;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthServer.EndSession;
internal class EndSessionEndpointHandler : IEndpointHandler
{
    private readonly IRequestAccessor<EndSessionRequest> _requestAccessor;
    private readonly IRequestHandler<EndSessionRequest, Unit> _requestHandler;
    private readonly IOptionsSnapshot<UserInteraction> _userInteractionOptions;
    private readonly IEndSessionUserAccessor _endSessionUserAccessor;

    public EndSessionEndpointHandler(
        IRequestAccessor<EndSessionRequest> requestAccessor,
        IRequestHandler<EndSessionRequest, Unit> requestHandler,
        IOptionsSnapshot<UserInteraction> userInteractionOptions,
        IEndSessionUserAccessor endSessionUserAccessor)
    {
        _requestAccessor = requestAccessor;
        _requestHandler = requestHandler;
        _userInteractionOptions = userInteractionOptions;
        _endSessionUserAccessor = endSessionUserAccessor;
    }

    public async Task<IResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var request = await _requestAccessor.GetRequest(httpContext.Request);
        var response = await _requestHandler.Handle(request, cancellationToken);
        _endSessionUserAccessor.ClearUser();
        return response.Match(
            _ =>
            {
                if (string.IsNullOrEmpty(request.PostLogoutRedirectUri))
                {
                    return Results.Ok();
                }

                var encodedState = HttpUtility.UrlEncode(request.State);
                return Results.Extensions.OAuthSeeOtherRedirect($"{request.PostLogoutRedirectUri}?state={encodedState}", httpContext.Response);
            },
            error => error switch
            {
                { Error: ErrorCode.InteractionRequired } => Results.Extensions.LocalRedirect(_userInteractionOptions.Value.EndSessionUri, httpContext),
                _ => Results.Extensions.OAuthBadRequest(error)
            });
    }
}
