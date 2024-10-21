using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Userinfo;
internal class UserinfoEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapMethods("connect/userinfo", ["GET", "POST"], (HttpContext httpContext, [FromKeyedServices("Userinfo")] IEndpointHandler endpointHandler, CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken))
            .WithDisplayName("OpenId Connect Userinfo")
            .WithName("OpenId Connect Userinfo")
            .WithDescription("Endpoint to get userinfo")
            .WithGroupName("Userinfo")
            .RequireAuthorization(AuthorizationConstants.Userinfo);
    }
}
