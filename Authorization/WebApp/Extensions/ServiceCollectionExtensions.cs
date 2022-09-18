using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Reflection;
using Application.Validation;
using Domain;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using WebApp.Constants;
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
    services.AddAuthorization(options =>
    {
      options.AddPolicy(AuthorizationConstants.ClientRegistration, policy =>
      {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimNameConstants.Scope, ScopeConstants.ClientRegistration);
      });
      options.AddPolicy(AuthorizationConstants.ClientConfiguration, policy =>
      {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimNameConstants.Scope, ScopeConstants.ClientConfiguration);
      });
      options.AddPolicy(AuthorizationConstants.ResourceRegistration, policy =>
      {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimNameConstants.Scope, ScopeConstants.ResourceRegistration);
      });
      options.AddPolicy(AuthorizationConstants.ResourceConfiguration, policy =>
      {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimNameConstants.Scope, ScopeConstants.ResourceConfiguration);
      });
    });
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

  public static IServiceCollection AddEndpointValidators(this IServiceCollection services)
  {
    foreach (var validator in Assembly.GetExecutingAssembly().GetTypes()
               .Where(x => x.GetInterface(typeof(IValidator<>).Name) is not null))
    {
      services.AddScoped(validator.GetInterface(typeof(IValidator<>).Name)!, validator);
    }

    return services;
  }
}
