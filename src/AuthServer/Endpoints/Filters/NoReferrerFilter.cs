using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AuthServer.Endpoints.Filters;

internal class NoReferrerFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        context.HttpContext.Response.Headers.Add(new KeyValuePair<string, StringValues>("Referrer-Policy", "no-referrer"));
        return await next(context);
    }
}