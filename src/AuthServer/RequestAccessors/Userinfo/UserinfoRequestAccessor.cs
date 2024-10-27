using AuthServer.Authentication.OAuthToken;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.Userinfo;
internal class UserinfoRequestAccessor : IRequestAccessor<UserinfoRequest>
{
    public async Task<UserinfoRequest> GetRequest(HttpRequest httpRequest)
    {
        var token = (await httpRequest.HttpContext.GetTokenAsync(OAuthTokenAuthenticationDefaults.AuthenticationScheme, Parameter.AccessToken))!;
        return new UserinfoRequest
        {
            AccessToken = token
        };
    }
}