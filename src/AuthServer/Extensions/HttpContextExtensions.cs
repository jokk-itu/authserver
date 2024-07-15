using AuthServer.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Extensions;
internal static class HttpContextExtensions
{
    public static async Task<string> GetToken(this HttpContext httpContext, string tokenName)
    {
        var referenceToken = await httpContext.GetTokenAsync(ReferenceTokenAuthenticationDefaults.AuthenticationScheme, tokenName);
        if (referenceToken is not null)
        {
            return referenceToken;
        }

        var jwt = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, tokenName);
        if (jwt is not null)
        {
            return jwt;
        }

        throw new InvalidOperationException("token could not be retrieved");
    }
}
