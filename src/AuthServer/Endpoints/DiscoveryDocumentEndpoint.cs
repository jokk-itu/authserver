using AuthServer.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthServer.Endpoints;
internal static class DiscoveryDocumentEndpoint
{
    public static Task<IResult> HandleDiscoveryDocument(
        [FromServices] IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        return Task.FromResult(Results.Ok(discoveryDocumentOptions.Value));
    }
}