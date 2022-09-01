using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Serilog;

namespace WebApp.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddOpenIdAuthentication(this IServiceCollection services, IdentityConfiguration identityConfiguration)
  {
    services
      .AddAuthentication(configureOptions =>
      {
        configureOptions.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
        configureOptions.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(OpenIdConnectDefaults.AuthenticationScheme, config =>
      {
        config.IncludeErrorDetails = true; //DEVELOP READY
        config.RequireHttpsMetadata = false; //DEVELOP READY
        config.MetadataAddress = $"{identityConfiguration.InternalIssuer}/.well-known/openid-configuration";
        config.Audience = identityConfiguration.Audience;
        config.Authority = identityConfiguration.InternalIssuer;
        config.SaveToken = true;
        config.Events = new JwtBearerEvents 
        {
          OnAuthenticationFailed = context => 
          {
            Log.Error(context.Exception, "Authentication failed");
            return Task.CompletedTask;
          },
          OnForbidden = context =>
          {
            Log.Information("Forbidden");
            return Task.CompletedTask;
          },
          OnTokenValidated = context =>
          {
            Log.Information("Token Validated");
            return Task.CompletedTask;
          }
        };
        config.Validate();
      });
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
