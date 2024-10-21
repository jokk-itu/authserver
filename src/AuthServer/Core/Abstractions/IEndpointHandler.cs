using Microsoft.AspNetCore.Http;

namespace AuthServer.Core.Abstractions;
internal interface IEndpointHandler
{
    Task<IResult> Handle(HttpContext httpContext, CancellationToken cancellationToken);
}