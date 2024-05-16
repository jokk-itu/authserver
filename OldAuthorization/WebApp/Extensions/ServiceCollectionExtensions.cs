using Microsoft.AspNetCore.Authentication.JwtBearer;
using Domain.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApp.Constants;
using WebApp.Options;
using WebApp.Context.Abstract;
using WebApp.Context.AuthorizeContext;
using WebApp.Context.ClientContext;
using WebApp.Context.EndSessionContext;
using WebApp.Context.IntrospectionContext;
using WebApp.Context.RevocationContext;
using WebApp.Context.TokenContext;

namespace WebApp.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddOpenIdAuthentication(this IServiceCollection services)
  {
    services.AddSingleton<InternalConfigurationManager>();
    services.ConfigureOptions<ConfigureJwtBearerOptions>();
    services.ConfigureOptions<ConfigureCookieAuthenticationOptions>();
    services
      .AddAuthentication(configureOptions =>
      {
        configureOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer()
      .AddCookie();
    return services;
  }

  public static IServiceCollection AddOpenIdAuthorization(this IServiceCollection services)
  {
    services.AddAuthorization(options =>
    {
      options.AddPolicy(AuthorizationConstants.UserInfo, policy =>
      {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAssertion(context =>
        {
          var scope = context.User.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Scope)?.Value;
          return scope is not null && scope.Split(' ').Contains(ScopeConstants.UserInfo);
        });
      });
      options.AddPolicy(AuthorizationConstants.ClientConfiguration, policy =>
      {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimNameConstants.Scope, ScopeConstants.ClientConfiguration);
      });
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

  public static IServiceCollection AddContextAccessors(this IServiceCollection services)
  {
    services.AddScoped<IContextAccessor<TokenContext>, TokenContextAccessor>();
    services.AddScoped<IContextAccessor<AuthorizeContext>, AuthorizeContextAccessor>();
    services.AddScoped<IContextAccessor<EndSessionContext>, EndSessionContextAccessor>();
    services.AddScoped<IContextAccessor<IntrospectionContext>, IntrospectionContextAccessor>();
    services.AddScoped<IContextAccessor<RevocationContext>, RevocationContextAccessor>();
    services.AddScoped<IContextAccessor<ClientContext>, ClientContextAccessor>();
    return services;
  }
}