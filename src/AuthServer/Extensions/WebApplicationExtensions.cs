using AuthServer.Core.Abstractions;
using AuthServer.Endpoints;
using AuthServer.Endpoints.Filters;
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
        app.UseMiddleware<OAuthErrorMiddleware>();

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
            .WithGroupName("Configuration")
            .AddEndpointFilter(async (ctx, next) =>
            {
                ctx.HttpContext.Response.Headers.CacheControl = "max-age=86400, public, must-revalidate";
                return await next(ctx);
            })
            .AddEndpointFilter<NoReferrerFilter>();

        endpointBuilder
            .MapMethods(".well-known/jwks", ["GET"], JwksDocumentEndpoint.HandleJwksDocument)
            .WithDisplayName("OAuth JWKS")
            .WithName("OAuth JWKS")
            .WithDescription("Endpoint to get the jwks")
            .WithGroupName("Configuration")
            .AddEndpointFilter<NoCacheFilter>()
            .AddEndpointFilter<NoReferrerFilter>();
    }
}
