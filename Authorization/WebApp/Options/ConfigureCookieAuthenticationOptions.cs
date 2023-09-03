using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using WebApp.Constants;
using CookieBuilder = Microsoft.AspNetCore.Http.CookieBuilder;

namespace WebApp.Options;

public class ConfigureCookieAuthenticationOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
{
  public void Configure(string name, CookieAuthenticationOptions options)
  {
    Configure(options);
  }

  public void Configure(CookieAuthenticationOptions options)
  {
    options.Cookie = new CookieBuilder
    {
      Name = CookieConstants.IdentityCookie,
      HttpOnly = true,
      SameSite = SameSiteMode.Strict,
      SecurePolicy = CookieSecurePolicy.Always,
      MaxAge = TimeSpan.FromDays(2),
      IsEssential = true
    };
  }
}