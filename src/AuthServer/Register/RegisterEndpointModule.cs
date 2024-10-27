using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Register;

internal class RegisterEndpointModule : IEndpointModule
{
    public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapMethods("connect/register", ["POST"], (HttpContext httpContext, [FromKeyedServices("Register")] IEndpointHandler endpointHandler, CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken))
            .WithDisplayName("OpenId Connect Dynamic Registration")
            .WithName("OpenId Connect Dynamic Registration")
            .WithDescription("Endpoint to register a client")
            .WithGroupName("Register");

        endpointRouteBuilder
            .MapMethods("connect/register", ["GET", "PUT", "DELETE"], (HttpContext httpContext, [FromKeyedServices("Register")] IEndpointHandler endpointHandler, CancellationToken cancellationToken) => endpointHandler.Handle(httpContext, cancellationToken))
            .WithDisplayName("OpenId Connect Dynamic Management")
            .WithName("OpenId Connect Dynamic Management")
            .WithDescription("Endpoint to manage a client")
            .WithGroupName("Register")
            .RequireAuthorization(AuthorizationConstants.Register);
    }
}