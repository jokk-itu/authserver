using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.Userinfo;
internal class UserinfoRequestAccessor : IRequestAccessor<UserinfoRequest>
{
    public async Task<UserinfoRequest> GetRequest(HttpRequest httpRequest)
    {
        var token = await httpRequest.HttpContext.GetToken(Parameter.AccessToken);
        return new UserinfoRequest
        {
            AccessToken = token
        };
    }
}