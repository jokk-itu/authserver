using System.Net.Http.Headers;
using System.Text;
using WebApp.Constants;
using WebApp.Context.Abstract;

namespace WebApp.Context.IntrospectionContext;

public class IntrospectionContextAccessor : IContextAccessor<IntrospectionContext>
{
  public async Task<IntrospectionContext> GetContext(HttpContext httpContext)
  {
    var context = new IntrospectionContext();
    var body = await httpContext.Request.ReadFormAsync();
    if (body.TryGetValue(ParameterNames.Token, out var token))
    {
      context.Token = token;
    }
    if (body.TryGetValue(ParameterNames.TokenTypeHint, out var tokenTypeHint))
    {
      context.TokenTypeHint = tokenTypeHint;
    }
    if (body.TryGetValue(ParameterNames.ClientId, out var clientId))
    {
      context.ClientId = clientId;
    }
    if (body.TryGetValue(ParameterNames.ClientSecret, out var clientSecret))
    {
      context.ClientSecret = clientSecret;
    }

    var isBasicAuthentication = AuthenticationHeaderValue.TryParse(httpContext.Request.Headers.Authorization, out var authenticationHeader);
    if (isBasicAuthentication
        && authenticationHeader?.Scheme == "Basic"
        && !string.IsNullOrWhiteSpace(authenticationHeader.Parameter))
    {
      var decodedBytes = Convert.FromBase64String(authenticationHeader.Parameter);
      var decoded = Encoding.UTF8.GetString(decodedBytes).Split(":");
      if (decoded.Length == 2)
      {
        context.ClientId = decoded[0];
        context.ClientSecret = decoded[1];
      }
    }

    return context;
  }
}
