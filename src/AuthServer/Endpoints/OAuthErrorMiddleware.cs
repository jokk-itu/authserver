using AuthServer.Core;
using AuthServer.Endpoints.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthServer.Endpoints;
internal class OAuthErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OAuthErrorMiddleware> _logger;

    public OAuthErrorMiddleware(
        RequestDelegate next,
        ILogger<OAuthErrorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unhandled error occurred");

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var error = new OAuthError(ErrorCode.ServerError, "unhandled error occurred");
            await context.Response.WriteAsJsonAsync(error, context.RequestAborted);
        }
    }
}
