using WebApp.Constants;
using WebApp.Context.Abstract;
using WebApp.Extensions;

namespace WebApp.Context.TokenContext;

public class TokenContextAccessor : IContextAccessor<TokenContext>
{
  public async Task<TokenContext> GetContext(HttpContext httpContext)
  {
    var context = new TokenContext();
    var body = await httpContext.Request.ReadFormAsync();
    if (body.TryGetValue(ParameterNames.GrantType, out var grantType))
    {
      context.GrantType = grantType;
    }

    var clientSecretBasic = httpContext.GetClientSecretBasic();
    if (clientSecretBasic is not null)
    {
      context.ClientAuthentications.Add(clientSecretBasic);
    }

    var clientSecretPost = body.GetClientSecretPost();
    if (clientSecretPost is not null)
    {
      context.ClientAuthentications.Add(clientSecretPost);
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

    if (body.TryGetValue(ParameterNames.Resource, out var resource))
    {
      // TODO temporary to conform to AspNet Core clients where multiple Resource parameters cannot be supplied
      context.Resource = resource.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    return context;
  }
}