using AuthorizationServer.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthorizationServer.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatastore(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<ClientManager>();
    services.AddScoped<ResourceManager>();
    services.AddDbContext<IdentityContext>(options =>
    {
      options.UseSqlServer(configuration.GetConnectionString("SqlServer"),
              optionsBuilder => { optionsBuilder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null); });
    });

    services.AddScoped<IdentityContext>();

    services.AddIdentityCore<IdentityUser>()
        .AddRoles<IdentityRole>()
        .AddDefaultTokenProviders()
        .AddEntityFrameworkStores<IdentityContext>();

    return services;
  }
}