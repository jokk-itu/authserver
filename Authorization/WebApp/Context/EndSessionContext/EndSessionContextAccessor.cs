using WebApp.Constants;
using WebApp.Context.Abstract;

namespace WebApp.Context.EndSessionContext;

public class EndSessionContextAccessor : IContextAccessor<EndSessionContext>
{
  public async Task<EndSessionContext> GetContext(HttpContext httpContext)
  {
    var context = httpContext.Request.Method == HttpMethod.Get.ToString() ? GetContextFromHeaders(httpContext) : await GetContextFromBody(httpContext);
    return context;
  }

  private static async Task<EndSessionContext> GetContextFromBody(HttpContext httpContext)
  {
    var context = new EndSessionContext();
    var formCollection = await httpContext.Request.ReadFormAsync();
    if (formCollection.TryGetValue(ParameterNames.IdTokenHint, out var idTokenHint))
    {
      context.IdTokenHint = idTokenHint;
    }

    if (formCollection.TryGetValue(ParameterNames.ClientId, out var clientId))
    {
      context.ClientId = clientId;
    }

    if (formCollection.TryGetValue(ParameterNames.PostLogoutRedirectUri, out var postLogoutRedirectUri))
    {
      context.PostLogoutRedirectUri = postLogoutRedirectUri;
    }

    if (formCollection.TryGetValue(ParameterNames.State, out var state))
    {
      context.State = state;
    }

    return context;
  }

  private static EndSessionContext GetContextFromHeaders(HttpContext httpContext)
  {
    var context = new EndSessionContext();
    var headers = httpContext.Request.Headers;
    if (headers.TryGetValue(ParameterNames.IdTokenHint, out var idTokenHint))
    {
      context.IdTokenHint = idTokenHint;
    }

    if (headers.TryGetValue(ParameterNames.ClientId, out var clientId))
    {
      context.ClientId = clientId;
    }

    if (headers.TryGetValue(ParameterNames.PostLogoutRedirectUri, out var postLogoutRedirectUri))
    {
      context.PostLogoutRedirectUri = postLogoutRedirectUri;
    }

    if (headers.TryGetValue(ParameterNames.State, out var state))
    {
      context.State = state;
    }

    return context;
  }
}
