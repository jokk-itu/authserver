using System.Text;
using WebApp.Constants;
using WebApp.Context.Abstract;

namespace WebApp.Context.TokenContext;

public class TokenContextAccessor : IContextAccessor<TokenContext>
{
    public async Task<TokenContext> GetContext(HttpContext httpContext)
    {
        var context = new TokenContext();
        var basicAuthorization = httpContext.Request.Headers.Authorization;
        var body = await httpContext.Request.ReadFormAsync();
        if (body.TryGetValue(ParameterNames.GrantType, out var grantType))
        {
            context.GrantType = grantType;
        }
        if (body.TryGetValue(ParameterNames.ClientId, out var clientId))
        {
            context.ClientId = clientId;
        }
        if (body.TryGetValue(ParameterNames.ClientSecret, out var clientSecret))
        {
            context.ClientSecret = clientSecret;
        }
        if (basicAuthorization.FirstOrDefault() == "Basic" && basicAuthorization.Count == 2)
        {
            var decodedBytes = Convert.FromBase64String(basicAuthorization[1]);
            var decoded = Encoding.UTF8.GetString(decodedBytes).Split(":");
            if (decoded.Length == 2)
            {
                context.ClientId = decoded[0];
                context.ClientSecret = decoded[1];
            }
        }
        if (body.TryGetValue(ParameterNames.Code, out var code))
        {
            context.Code = code;
        }
        if (body.TryGetValue(ParameterNames.CodeVerifier, out var codeVerifier))
        {
            context.CodeVerifier = codeVerifier;
        }
        if (body.TryGetValue(ParameterNames.RedirectUri, out var redirectUri))
        {
            context.RedirectUri = redirectUri;
        }
        if (body.TryGetValue(ParameterNames.RefreshToken, out var refreshToken))
        {
            context.RefreshToken = refreshToken;
        }
        if (body.TryGetValue(ParameterNames.Scope, out var scope))
        {
            context.Scope = scope;
        }

        return context;
    }
}
