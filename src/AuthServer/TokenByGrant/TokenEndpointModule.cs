using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.TokenByGrant;
internal class TokenEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapMethods("connect/token", ["POST"], (HttpContext httpContext, [FromKeyedServices("Token")] IEndpointHandler endpointHandler, CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken))
            .WithDisplayName("OpenId Connect Token")
            .WithName("OpenId Connect Token")
            .WithDescription("Endpoint to get tokens")
            .WithGroupName("Token");
    }
}
