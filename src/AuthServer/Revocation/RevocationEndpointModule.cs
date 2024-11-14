using AuthServer.Core.Abstractions;
using AuthServer.Endpoints.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Revocation;

internal class RevocationEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var routeBuilder = endpointRouteBuilder
            .MapMethods(
                "connect/revoke",
                ["POST"],
                (HttpContext httpContext, [FromKeyedServices("Revocation")] IEndpointHandler endpointHandler,
                    CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken));

        routeBuilder
            .WithDisplayName("OAuth Revocation")
            .WithName("OAuth Revocation")
            .WithDescription("Endpoint to revoke a given token")
            .WithGroupName("Revocation");

        routeBuilder
            .AddEndpointFilter<NoCacheFilter>()
            .AddEndpointFilter<NoReferrerFilter>();
    }
}