using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using WebApp.Context.Abstract;

namespace WebApp.Context.ClientContext;

public class ClientContextAccessor : IContextAccessor<ClientContext>
{
  private readonly IOptions<JsonOptions> _jsonOptions;

  public ClientContextAccessor(IOptions<JsonOptions> jsonOptions)
  {
    _jsonOptions = jsonOptions;
  }

  public async Task<ClientContext> GetContext(HttpContext httpContext)
  {
    return await httpContext.Request.ReadFromJsonAsync<ClientContext>(_jsonOptions.Value.SerializerOptions)
           ?? new ClientContext();
  }
}