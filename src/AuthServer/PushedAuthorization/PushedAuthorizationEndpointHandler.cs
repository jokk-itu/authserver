using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Endpoints.Responses;
using AuthServer.Extensions;
using AuthServer.Options;
using AuthServer.RequestAccessors.PushedAuthorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthServer.PushedAuthorization;

internal class PushedAuthorizationEndpointHandler : IEndpointHandler
{
    private readonly IRequestAccessor<PushedAuthorizationRequest> _requestAccessor;
    private readonly IRequestHandler<PushedAuthorizationRequest, PushedAuthorizationResponse> _requestHandler;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    public PushedAuthorizationEndpointHandler(
        IRequestAccessor<PushedAuthorizationRequest> requestAccessor,
        IRequestHandler<PushedAuthorizationRequest, PushedAuthorizationResponse> requestHandler,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _requestAccessor = requestAccessor;
        _requestHandler = requestHandler;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public async Task<IResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var request = await _requestAccessor.GetRequest(httpContext.Request);
        var response = await _requestHandler.Handle(request, cancellationToken);
        return response.Match(
            result =>
            {
                var uri =
                    $"{_discoveryDocumentOptions.Value.AuthorizationEndpoint}?{Parameter.RequestUri}={result.RequestUri}&{Parameter.ClientId}={result.ClientId}";

                var location = new Uri(uri, UriKind.Absolute);
                return Results.Created(location,
                    new PostPushedAuthorizationResponse
                    {
                        RequestUri = result.RequestUri,
                        ExpiresIn = result.ExpiresIn
                    });
            },
            error => Results.Extensions.OAuthBadRequest(error));
    }
}