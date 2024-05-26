using System.Text;
using System.Text.Json;
using AuthServer.Constants;
using AuthServer.Core;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Extensions;
public static class ResultsExtensions
{
    public static IResult OAuthBadRequest(this IResultExtensions _, OAuthError error)
    {
        return Results.Text(JsonSerializer.Serialize(error), MimeTypeConstants.Json, Encoding.UTF8, StatusCodes.Status400BadRequest);
    }

    public static IResult OAuthOkWithHtml(this IResultExtensions _, string html)
    {
        return Results.Text(html, MimeTypeConstants.Html, Encoding.UTF8, StatusCodes.Status200OK);
    }
}