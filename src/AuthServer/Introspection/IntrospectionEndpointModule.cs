using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Introspection;
internal class IntrospectionEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapMethods("connect/introspection", ["POST"], (HttpContext httpContext, [FromKeyedServices("Introspection")] IEndpointHandler endpointHandler, CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken))
            .WithDisplayName("OAuth Introspection")
            .WithName("OAuth Introspection")
            .WithDescription("Endpoint to get a structured token from a reference")
            .WithGroupName("Introspection");
    }
}
