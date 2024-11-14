using AuthServer.Core.Abstractions;
using AuthServer.Endpoints.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.EndSession;

internal class EndSessionEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var routeBuilder = endpointRouteBuilder.MapMethods(
            "connect/end-session",
            ["GET", "POST"],
            (HttpContext httpContext, [FromKeyedServices("EndSession")] IEndpointHandler endpointHandler,
                CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken));

        routeBuilder
            .WithDisplayName("OpenId Connect EndSession")
            .WithName("OpenId Connect EndSession")
            .WithDescription("Endpoint to end the session")
            .WithGroupName("EndSession");

        routeBuilder
            .AddEndpointFilter<NoCacheFilter>()
            .AddEndpointFilter<NoReferrerFilter>();
    }
}