﻿using AuthServer.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.Userinfo;
internal class UserinfoRequestAccessor : IRequestAccessor<UserinfoRequest>
{
    public async Task<UserinfoRequest> GetRequest(HttpRequest httpRequest)
    {
        var token = await httpRequest.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Parameter.AccessToken);
        return new UserinfoRequest
        {
            AccessToken = token ?? string.Empty
        };
    }
}