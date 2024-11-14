using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Endpoints.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Userinfo;

internal class UserinfoEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var routeBuilder = endpointRouteBuilder
            .MapMethods(
                "connect/userinfo",
                ["GET", "POST"],
                (HttpContext httpContext, [FromKeyedServices("Userinfo")] IEndpointHandler endpointHandler,
                    CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken));

        routeBuilder
            .WithDisplayName("OpenId Connect Userinfo")
            .WithName("OpenId Connect Userinfo")
            .WithDescription("Endpoint to get userinfo")
            .WithGroupName("Userinfo");

        routeBuilder.RequireAuthorization(AuthorizationConstants.Userinfo);

        routeBuilder
            .AddEndpointFilter<NoCacheFilter>()
            .AddEndpointFilter<NoReferrerFilter>();
    }
}