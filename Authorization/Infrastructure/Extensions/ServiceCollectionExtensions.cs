using AuthorizationServer.Repositories;
using AuthorizationServer.TokenFactories;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthorizationServer.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatastore(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDataProtection();
    services.AddTransient<AuthorizationCodeTokenFactory>();
    services.AddTransient<AccessTokenFactory>();
    services.AddTransient<IdTokenFactory>();
    services.AddTransient<RefreshTokenFactory>();

    services.AddScoped<ClientManager>();
    services.AddScoped<ResourceManager>();
    services.AddScoped<ScopeManager>();

    services.AddSingleton<JwkManager>();

    services.AddDbContext<IdentityContext>(options =>
    {
      options.UseSqlServer(configuration.GetConnectionString("SqlServer"),
              optionsBuilder =>
              {
                optionsBuilder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
                optionsBuilder.MigrationsAssembly("Infrastructure");
              });
    });
    services.AddScoped<IdentityContext>();

    services.AddIdentityCore<IdentityUser>()
        .AddRoles<IdentityRole>()
        .AddDefaultTokenProviders()
        .AddEntityFrameworkStores<IdentityContext>()
        .AddSignInManager<SignInManager<IdentityUser>>();

    return services;
  }
}