using Infrastructure.Repositories;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Application.Validation;
using MediatR;
using Infrastructure.Builders;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Services;
using Infrastructure.Services.Abstract;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDataStore(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDataProtection();

    services.AddDbContext<IdentityContext>(options =>
    {
      var sqliteConnection = configuration.GetConnectionString("SQLite");
      var sqlServerConnection = configuration.GetConnectionString("SqlServer");

      if (!string.IsNullOrWhiteSpace(sqliteConnection))
      {
        options.UseSqlite(sqliteConnection, optionsBuilder =>
        {
          optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
          optionsBuilder.MigrationsAssembly(typeof(IdentityContext).Namespace);
        });
      }
      else if (!string.IsNullOrWhiteSpace(sqlServerConnection))
      {
        options.UseSqlServer(sqlServerConnection, optionsBuilder =>
        {
          optionsBuilder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
          optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
          optionsBuilder.MigrationsAssembly(typeof(IdentityContext).Namespace);
        });
      }
      else
      {
        throw new Exception("ConnectionString is not provided");
      }
    });
    services.AddScoped<IdentityContext>();

    services.AddIdentityCore<User>()
        .AddRoles<IdentityRole>()
        .AddDefaultTokenProviders()
        .AddEntityFrameworkStores<IdentityContext>();

    return services;
  }

  public static IServiceCollection AddDataServices(this IServiceCollection services)
  {
    services.AddTransient<IClaimService, ClaimService>();
    return services;
  }

  public static IServiceCollection AddBuilders(this IServiceCollection services)
  {
    services.AddTransient<ITokenBuilder, TokenBuilder>();
    services.AddTransient<ICodeBuilder, CodeBuilder>();
    services.AddTransient<IDiscoveryBuilder, DiscoveryBuilder>();
    services.AddTransient<IFormPostBuilder, FormPostBuilder>();
    return services;
  }

  public static IServiceCollection AddDecoders(this IServiceCollection services)
  {
    services.AddTransient<ITokenDecoder, TokenDecoder>();
    services.AddTransient<ICodeDecoder, CodeDecoder>();
    return services;
  }

  public static IServiceCollection AddManagers(this IServiceCollection services)
  {
    services.AddScoped<ResourceManager>();
    services.AddSingleton<JwkManager>();
    return services;
  }

  public static IServiceCollection AddRequests(this IServiceCollection services)
  {
    AddBaseValidators(services);
    AddValidators(services);
    services.AddMediatR(Assembly.GetExecutingAssembly());
    return services;
  }

  private static void AddValidators(IServiceCollection services)
  {
    var validators = Assembly
      .GetExecutingAssembly()
      .GetTypes()
      .Where(x => x.IsClass
                  && x.GetInterface(typeof(IValidator<>).Name) is not null)
      .ToList();

    foreach (var validator in validators)
    {
      var serviceType = validator.GetInterface(typeof(IValidator<>).Name)!;
      services.AddScoped(serviceType, validator);
    }
  }

  private static void AddBaseValidators(IServiceCollection services)
  {
    var validators = Assembly
      .GetExecutingAssembly()
      .GetTypes()
      .Where(x => x.IsClass
                  && x.GetInterface(typeof(IBaseValidator<>).Name) is not null)
      .ToList();

    foreach (var validator in validators)
    {
      var serviceType = validator.GetInterface(typeof(IBaseValidator<>).Name)!;
      services.AddScoped(serviceType, validator);
    }
  }
}