using AuthServer.Core;
using AuthServer.RequestAccessors.Register;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Endpoints;
internal class PostRegisterEndpoint
{
    public static async Task<IResult> HandlePostRegister(
        HttpContext httpContext,
        [FromServices] IRequestAccessor<RegisterRequest> requestAccessor,
        CancellationToken cancellationToken)
    {
        var request = await requestAccessor.GetRequest(httpContext.Request);
        throw new NotImplementedException();
    }
}