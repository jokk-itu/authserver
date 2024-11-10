using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.PushedAuthorization;

internal class PushedAuthorizationEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapMethods("connect/par", ["POST"],
                (HttpContext httpContext, [FromKeyedServices("PushedAuthorization")] IEndpointHandler endpointHandler,
                    CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken))
            .WithDisplayName("OpenId Connect Pushed Authorization")
            .WithName("OpenId Connect Pushed Authorization")
            .WithDescription("OpenId Connect Pushed Authorization")
            .WithGroupName("Authorize");
    }
}