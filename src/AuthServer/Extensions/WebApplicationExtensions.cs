using AuthServer.Core.Abstractions;
using AuthServer.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthServer.Extensions;
public static class WebApplicationExtensions
{
    public static void UseAuthServer(this WebApplication app)
    {
        var modules = app.Services.GetServices<IEndpointModule>();
        foreach (var module in modules)
        {
            module.RegisterEndpoint(app);
            app.Logger.LogDebug("Registered endpoint {Endpoint}", nameof(module));
        }

        IEndpointRouteBuilder endpointBuilder = app;

        endpointBuilder
            .MapMethods(".well-known/openid-configuration", ["GET"], DiscoveryDocumentEndpoint.HandleDiscoveryDocument)
            .WithDisplayName("OpenIdConnect Configuration")
            .WithName("OpenIdConnect Configuration")
            .WithDescription("Endpoint to get the configuration")
            .WithGroupName("Configuration");

        endpointBuilder
            .MapMethods(".well-known/jwks", ["GET"], JwksDocumentEndpoint.HandleJwksDocument)
            .WithDisplayName("OAuth JWKS")
            .WithName("OAuth JWKS")
            .WithDescription("Endpoint to get the jwks")
            .WithGroupName("Configuration");
    }
}
