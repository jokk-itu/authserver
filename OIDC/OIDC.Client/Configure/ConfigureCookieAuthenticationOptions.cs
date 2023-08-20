using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
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
    options.Cookie = new CookieBuilder
    {
      HttpOnly = true,
      SameSite = SameSiteMode.Strict,
      SecurePolicy = CookieSecurePolicy.Always,
      IsEssential = true,
    };
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
