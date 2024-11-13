using Microsoft.AspNetCore.Http;

namespace AuthServer.Endpoints.Filters;

internal class NoCacheFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        context.HttpContext.Response.Headers.CacheControl = "no-cache, no-store";
        return await next(context);
    }
}