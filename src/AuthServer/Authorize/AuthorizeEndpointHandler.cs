using AuthServer.Authorize.Abstractions;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.Options;
using AuthServer.RequestAccessors.Authorize;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthServer.Authorize;
internal class AuthorizeEndpointHandler : IEndpointHandler
{
    private readonly IRequestAccessor<AuthorizeRequest> _requestAccessor;
    private readonly IRequestHandler<AuthorizeRequest, string> _requestHandler;
    private readonly IAuthorizeResponseBuilder _authorizeResponseBuilder;
    private readonly IOptionsSnapshot<UserInteraction> _userInteractionOptions;
    private readonly IAuthorizeUserAccessor _authorizeUserAccessor;

    public AuthorizeEndpointHandler(
        IRequestAccessor<AuthorizeRequest> requestAccessor,
        IRequestHandler<AuthorizeRequest, string> requestHandler,
        IAuthorizeResponseBuilder authorizeResponseBuilder,
        IOptionsSnapshot<UserInteraction> userInteractionOptions,
        IAuthorizeUserAccessor authorizeUserAccessor)
    {
        _requestAccessor = requestAccessor;
        _requestHandler = requestHandler;
        _authorizeResponseBuilder = authorizeResponseBuilder;
        _userInteractionOptions = userInteractionOptions;
        _authorizeUserAccessor = authorizeUserAccessor;
    }

    public async Task<IResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var request = await _requestAccessor.GetRequest(httpContext.Request);
        var response = await _requestHandler.Handle(request, cancellationToken);

        return await response.Match(
            async code =>
            {
                // remove the authorized user to reset the interaction flow
                _authorizeUserAccessor.ClearUser();
                return await _authorizeResponseBuilder.BuildResponse(request,
                    new Dictionary<string, string> { { Parameter.Code, code } }, httpContext.Response, cancellationToken);
            },
            async error => string.IsNullOrEmpty(request.Prompt) && error.Error is ErrorCode.LoginRequired or ErrorCode.ConsentRequired or ErrorCode.AccountSelectionRequired
                ? error switch
                {
                    { Error: ErrorCode.LoginRequired } => Results.Extensions.LocalRedirect(_userInteractionOptions.Value.LoginUri, httpContext),
                    { Error: ErrorCode.ConsentRequired } => Results.Extensions.LocalRedirect(_userInteractionOptions.Value.ConsentUri, httpContext),
                    _ => Results.Extensions.LocalRedirect(_userInteractionOptions.Value.AccountSelectionUri, httpContext)
                }
                // TODO invoke ClearUser
                : error switch
                {
                    { ResultCode: ResultCode.Redirect } => await _authorizeResponseBuilder.BuildResponse(request, error.ToDictionary(), httpContext.Response, cancellationToken),
                    _ => Results.Extensions.OAuthBadRequest(error)
                });
    }
}