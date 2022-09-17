using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using WebApp.Options;

namespace WebApp.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddOpenIdAuthentication(this IServiceCollection services)
  {
    services.AddSingleton<InternalConfigurationManager>();
    services.ConfigureOptions<ConfigureJwtBearerOptions>();
    services
      .AddAuthentication(configureOptions =>
      {
        configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        configureOptions.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer();
    return services;
  }

  public static IServiceCollection AddOpenIdAuthorization(this IServiceCollection services)
  {
    services.AddAuthorization();
    return services;
  }

  public static IServiceCollection AddCookiePolicy(this IServiceCollection services)
  {
    services.AddCookiePolicy(cookiePolicyOptions =>
    {
      cookiePolicyOptions.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
      cookiePolicyOptions.MinimumSameSitePolicy = SameSiteMode.None;
      cookiePolicyOptions.Secure = CookieSecurePolicy.Always;
    });
    return services;
  }

  public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
  {
    services.AddCors(corsOptions =>
    {
      corsOptions.AddDefaultPolicy(corsPolicyBuilder =>
      {
        corsPolicyBuilder
        .AllowAnyOrigin()
        .AllowAnyHeader();
      });
    });
    return services;
  }
}
