using Infrastructure.Repositories;
using Infrastructure.TokenFactories;
using Domain;
using Infrastructure.Factories.TokenFactories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using Infrastructure.PipelineBehaviours;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatastore(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDataProtection();
    services.AddTransient<CodeFactory>();
    services.AddTransient<AccessTokenFactory>();
    services.AddTransient<IdTokenFactory>();
    services.AddTransient<RefreshTokenFactory>();

    services.AddScoped<ClientManager>();
    services.AddScoped<ResourceManager>();
    services.AddScoped<ScopeManager>();
    services.AddScoped<TokenManager>();
    services.AddScoped<CodeManager>();
    services.AddScoped<NonceManager>();
    services.AddScoped<TestManager>();

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

    services.AddIdentityCore<User>()
        .AddRoles<IdentityRole>()
        .AddDefaultTokenProviders()
        .AddEntityFrameworkStores<IdentityContext>();

    return services;
  }
}