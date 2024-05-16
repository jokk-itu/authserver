using WebApp.Context.Abstract;

namespace WebApp.Context.ClientContext;

public class ClientContextAccessor : IContextAccessor<ClientContext>
{
  public async Task<ClientContext> GetContext(HttpContext httpContext)
  {
    return await httpContext.Request.ReadFromJsonAsync<ClientContext>()
      ?? new ClientContext();
  }
}