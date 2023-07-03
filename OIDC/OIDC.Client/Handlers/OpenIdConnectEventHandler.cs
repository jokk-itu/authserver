using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using OIDC.Client.Handlers.Abstract;

namespace OIDC.Client.Handlers;
public class OpenIdConnectEventHandler : IOpenIdConnectEventHandler
{
  public Task SetClientIdOnRedirect(RedirectContext context)
  {
    context.ProtocolMessage.Parameters.Add("client_id", context.Options.ClientId);
    return Task.CompletedTask;
  }
}