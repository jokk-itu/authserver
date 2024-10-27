using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Authorize;
internal class AuthorizeEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapMethods("connect/authorize", ["GET", "POST"], (HttpContext httpContext, [FromKeyedServices("Authorize")] IEndpointHandler endpointHandler, CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken))
            .WithDisplayName("OpenId Connect Authorize")
            .WithName("OpenId Connect Authorize")
            .WithDescription("OpenId Connect Authorize")
            .WithGroupName("Authorize");
    }
}
