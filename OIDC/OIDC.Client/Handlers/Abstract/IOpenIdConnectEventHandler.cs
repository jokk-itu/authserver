using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace OIDC.Client.Handlers.Abstract;
public interface IOpenIdConnectEventHandler
{
  Task SetClientIdOnRedirect(RedirectContext context);
}
