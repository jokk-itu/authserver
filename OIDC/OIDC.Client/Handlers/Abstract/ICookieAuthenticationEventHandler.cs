using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OIDC.Client.Handlers.Abstract;
public interface ICookieAuthenticationEventHandler
{
    Task GetRefreshTokenIfExceededExpiration(CookieValidatePrincipalContext context);
}
