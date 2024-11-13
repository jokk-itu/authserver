using System.Text;
using System.Text.Json;
using System.Web;
using AuthServer.Constants;
using AuthServer.Endpoints.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace AuthServer.Extensions;

public static class ResultExtensions
{
    public static IResult OAuthBadRequest(this IResultExtensions _, OAuthError error)
    {
        return Results.Text(JsonSerializer.Serialize(error), MimeTypeConstants.Json, Encoding.UTF8,
            StatusCodes.Status400BadRequest);
    }

    public static IResult OAuthOkWithHtml(this IResultExtensions _, string html)
    {
        return Results.Text(html, MimeTypeConstants.Html, Encoding.UTF8, StatusCodes.Status200OK);
    }

    public static IResult OAuthSeeOtherRedirect(this IResultExtensions _, string location, HttpResponse response)
    {
        response.Headers.Location = location;
        return Results.StatusCode(StatusCodes.Status303SeeOther);
    }

    public static IResult LocalRedirect(this IResultExtensions _, string url, HttpContext httpContext)
    {
        var rawUrl = string.Empty;
        if (httpContext.Request.Method == "GET")
        {
            rawUrl = httpContext.Request.GetDisplayUrl();
        }
        else if (httpContext.Request.Method == "POST")
        {
            var body = httpContext.Request.Form;
            var baseUrl = httpContext.Request.GetDisplayUrl();
            var query = new QueryBuilder(body).ToQueryString();
            rawUrl = baseUrl + query;
        }

        var returnUrl = HttpUtility.UrlEncode(rawUrl);
        var location = $"{url}?returnUrl={returnUrl}";
        return Results.Extensions.OAuthSeeOtherRedirect(location, httpContext.Response);
    }
}