using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Core.Request;
using AuthServer.Endpoints.Responses;
using AuthServer.Extensions;
using AuthServer.PushedAuthorization;
using AuthServer.RequestAccessors.PushedAuthorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthServer.Endpoints;

internal static class PushedAuthorizationRequestEndpoint
{
    public static async Task<IResult> HandlePushedAuthorization(
        HttpContext httpContext,
        [FromServices] IRequestAccessor<PushedAuthorizationRequest> requestAccessor,
        [FromServices] IRequestHandler<PushedAuthorizationRequest, PushedAuthorizationResponse> requestHandler,
        [FromServices] IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        CancellationToken cancellationToken)
    {
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var response = await requestHandler.Handle(request, cancellationToken);
        return response.Match(
            result =>
            {
                var uri =
                    $"{discoveryDocumentOptions.Value.AuthorizationEndpoint}?{Parameter.RequestUri}={result.RequestUri}&{Parameter.ClientId}={result.ClientId}";

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