using Infrastructure.Repositories;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Application.Validation;
using Infrastructure.Requests;
using MediatR;
using Infrastructure.Factories;
using Infrastructure.Builders;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders;
using Infrastructure.Decoders.Abstractions;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDataStore(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDataProtection();
    services.AddTransient<CodeFactory>();
    services.AddTransient<ITokenBuilder, TokenBuilder>();
    services.AddTransient<ITokenDecoder, TokenDecoder>();

    services.AddScoped<ClientManager>();
    services.AddScoped<ResourceManager>();
    services.AddScoped<ScopeManager>();
    services.AddScoped<TokenManager>();
    services.AddScoped<CodeManager>();
    services.AddScoped<NonceManager>();

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

  public static IServiceCollection AddRequests(this IServiceCollection services)
  {
    var validators = Assembly
      .GetExecutingAssembly()
      .GetTypes()
      .Where(x => x.IsClass
                  && x.GetInterface(typeof(IValidator<>).Name) is not null 
                  && x.Namespace!.Contains(typeof(Response).Namespace!)).ToList();
    foreach (var validator in validators)
    {
      var serviceType = validator.GetInterface(typeof(IValidator<>).Name)!;
      services.AddScoped(serviceType, validator);
    }

    services.AddMediatR(Assembly.GetExecutingAssembly());
    return services;
  }
}