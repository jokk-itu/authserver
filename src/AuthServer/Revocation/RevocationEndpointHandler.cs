using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Revocation;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Revocation;
internal class RevocationEndpointHandler : IEndpointHandler
{
    private readonly IRequestAccessor<RevocationRequest> _requestAccessor;
    private readonly IRequestHandler<RevocationRequest, Unit> _requestHandler;

    public RevocationEndpointHandler(
        IRequestAccessor<RevocationRequest> requestAccessor,
        IRequestHandler<RevocationRequest, Unit> requestHandler)
    {
        _requestAccessor = requestAccessor;
        _requestHandler = requestHandler;
    }

    public async Task<IResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var request = await _requestAccessor.GetRequest(httpContext.Request);
        var response = await _requestHandler.Handle(request, cancellationToken);
        return response.Match(
            _ => Results.Ok(),
            error => Results.Extensions.OAuthBadRequest(error));
    }
}
