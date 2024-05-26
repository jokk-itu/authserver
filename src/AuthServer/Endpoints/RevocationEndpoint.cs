using AuthServer.Core;
using AuthServer.Core.RequestProcessing;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Revocation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Endpoints;
internal static class RevocationEndpoint
{
    public static async Task<IResult> HandleRevocation(
        HttpContext httpContext,
        [FromServices] IRequestAccessor<RevocationRequest> requestAccessor,
        [FromServices] IRequestProcessor<RevocationRequest, Unit> requestProcessor,
        CancellationToken cancellationToken)
    {
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var response = await requestProcessor.Process(request, cancellationToken);
        return response.Match(
            _ => Results.Ok(),
            error => Results.Extensions.OAuthBadRequest(error));
    }
}