using System.Text.Json;
using AuthServer.Core.Discovery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthServer.Endpoints;
internal static class JwksDocumentEndpoint
{
    public static Task<IResult> HandleJwksDocument(
        [FromServices] IOptionsSnapshot<JwksDocument> jwksDocumentOptions)
    {
        return Task.FromResult(Results.Ok(JsonSerializer.Serialize(jwksDocumentOptions.Value.ConvertToJwks())));
    }
}