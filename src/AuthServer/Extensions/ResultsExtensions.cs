using System.Text;
using System.Text.Json;
using AuthServer.Constants;
using AuthServer.Core;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Extensions;
public static class ResultsExtensions
{
    public static IResult OAuthInternalServerError(this IResultExtensions _, OAuthError error)
    {
        return Results.Text(JsonSerializer.Serialize(error), MimeTypeConstants.Json, Encoding.UTF8, StatusCodes.Status500InternalServerError);
    }

    public static IResult OAuthBadRequest(this IResultExtensions _, OAuthError error)
    {
        return Results.Text(JsonSerializer.Serialize(error), MimeTypeConstants.Json, Encoding.UTF8, StatusCodes.Status400BadRequest);
    }
}