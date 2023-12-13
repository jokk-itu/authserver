using WebApp.Constants;
using WebApp.Context.Abstract;
using WebApp.Extensions;

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

    return context;
  }
}
