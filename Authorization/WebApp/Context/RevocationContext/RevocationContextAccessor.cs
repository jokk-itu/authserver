using System.Text;
using WebApp.Constants;
using WebApp.Context.Abstract;

namespace WebApp.Context.RevocationContext;

public class RevocationContextAccessor : IContextAccessor<RevocationContext>
{
  public async Task<RevocationContext> GetContext(HttpContext httpContext)
  {
    var context = new RevocationContext();
    var basicAuthorization = httpContext.Request.Headers.Authorization;
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

    return context;
  }
}
