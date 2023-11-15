using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Application.Validation;
using MediatR;
using Infrastructure.Builders;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.ClientAccessToken;
using Infrastructure.Builders.Token.GrantAccessToken;
using Infrastructure.Builders.Token.IdToken;
using Infrastructure.Builders.Token.LogoutToken;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Builders.Token.RegistrationToken;
using Infrastructure.Decoders;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.DelegatingHandlers;
using Infrastructure.PipelineBehaviors;
using Infrastructure.Services;
using Infrastructure.Services.Abstract;
using Microsoft.Extensions.Http;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDataStore(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDataProtection();

    services.AddDbContext<IdentityContext>(options =>
    {
      var sqliteConnection = configuration.GetConnectionString("SQLite");

      if (!string.IsNullOrWhiteSpace(sqliteConnection))
      {
        options.UseSqlite(sqliteConnection, optionsBuilder =>
        {
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
    return services;
  }

  public static IServiceCollection AddServices(this IServiceCollection services)
  {
    services.AddScoped<IClaimService, ClaimService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IClientService, ClientService>();
    services.AddScoped<INonceService, NonceService>();
    services.AddScoped<IScopeService, ScopeService>();
    services.AddScoped<IConsentGrantService, ConsentGrantService>();
    services.AddScoped<IAuthorizationGrantService, AuthorizationGrantService>();
    return services;
  }

  public static IServiceCollection AddBuilders(this IServiceCollection services)
  {
    services.AddTransient<ITokenBuilder<IdTokenArguments>, IdTokenBuilder>();
    services.AddTransient<ITokenBuilder<GrantAccessTokenArguments>, GrantAccessTokenBuilder>();
    services.AddTransient<ITokenBuilder<ClientAccessTokenArguments>, ClientAccessTokenBuilder>();
    services.AddTransient<ITokenBuilder<RefreshTokenArguments>, RefreshTokenBuilder>();
    services.AddTransient<ITokenBuilder<RegistrationTokenArguments>, RegistrationTokenBuilder>();
    services.AddTransient<ITokenBuilder<LogoutTokenArguments>, LogoutTokenBuilder>();
    services.AddTransient<ICodeBuilder, CodeBuilder>();
    services.AddTransient<IDiscoveryBuilder, DiscoveryBuilder>();
    return services;
  }

  public static IServiceCollection AddDecoders(this IServiceCollection services)
  {
    services.AddTransient<ICodeDecoder, CodeDecoder>();
    services.AddTransient<IStructuredTokenDecoder, StructuredTokenDecoder>();
    return services;
  }

  public static IServiceCollection AddManagers(this IServiceCollection services)
  {
    services.AddSingleton<JwkManager>();
    return services;
  }

  public static IServiceCollection AddRequests(this IServiceCollection services)
  {
    AddBaseValidators(services);
    AddValidators(services);
    AddPipelineBehavior(services);
    services.AddMediatR(Assembly.GetExecutingAssembly());
    return services;
  }

  public static IServiceCollection AddDelegatingHandlers(this IServiceCollection services)
  {
    services.AddHttpClient();
    services.AddTransient<PerformanceDelegatingHandler>();

    services.ConfigureAll<HttpClientFactoryOptions>(options =>
    {
      options.HttpMessageHandlerBuilderActions.Add(builder =>
      {
        builder.AdditionalHandlers.Add(builder.Services.GetRequiredService<PerformanceDelegatingHandler>());
      });
    });
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

  private static void AddPipelineBehavior(IServiceCollection services)
  {
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
  }
}