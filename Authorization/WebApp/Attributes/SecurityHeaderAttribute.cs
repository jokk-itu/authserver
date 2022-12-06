using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class SecurityHeaderAttribute : Attribute, IAsyncActionFilter
{
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    await next();

    var headers = context.HttpContext.Response.Headers;
    headers.CacheControl = "no-cache, no-store";
    headers.Pragma = "no-cache";
    headers.XContentTypeOptions = "DENY";
    headers.XFrameOptions = "SAMEORIGIN";
    headers.ContentSecurityPolicy = "default-src 'self'; script-src 'unsafe-inline'";
  }
}