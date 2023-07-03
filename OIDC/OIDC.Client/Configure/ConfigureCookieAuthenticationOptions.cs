using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using OIDC.Client.Handlers.Abstract;

namespace OIDC.Client.Configure;
public class ConfigureCookieAuthenticationOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
{
  private readonly ICookieAuthenticationEventHandler _cookieAuthenticationEventHandler;

  public ConfigureCookieAuthenticationOptions(ICookieAuthenticationEventHandler cookieAuthenticationEventHandler)
  {
    _cookieAuthenticationEventHandler = cookieAuthenticationEventHandler;
  }

  public void Configure(CookieAuthenticationOptions options)
  {
    options.Events = new CookieAuthenticationEvents
    {
      OnValidatePrincipal = _cookieAuthenticationEventHandler.GetRefreshTokenIfExceededExpiration
    };
  }

  public void Configure(string name, CookieAuthenticationOptions options)
  {
    Configure(options);
  }
}
