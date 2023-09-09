using Application;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Specs;
public abstract class BaseUnitTest
{
  protected readonly IdentityContext IdentityContext;

  protected BaseUnitTest()
  {
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<IdentityContext>()
      .UseSqlite(connection)
      .Options;

    IdentityContext = new IdentityContext(options);
    IdentityContext.Database.EnsureCreated();
  }

  private IServiceCollection ConfigureServices()
  {
    var services = new ServiceCollection();
    services.Configure<JwtBearerOptions>(config =>
    {
      config.Audience = "api";
      config.Authority = "auth-server";
    });
    services.AddDataProtection();
    services.AddHttpClient();

    services
      .AddSingleton(_ => new IdentityConfiguration
      {
        AccessTokenExpiration = 3600,
        IdTokenExpiration = 300,
        RefreshTokenExpiration = 26000,
        EncryptingKeySecret = "WnZr4u7w!z%C*F-J",
        CodeSecret = CryptographyHelper.GetRandomString(32),
        PrivateKeySecret = CryptographyHelper.GetRandomString(32),
        ServiceDocumentation = "http://localhost:5173",
        Issuer = "auth-server",
        UseReferenceTokens = false,
      })
      .AddScoped(_ => IdentityContext)
      .AddLogging()
      .AddBuilders()
      .AddServices()
      .AddDecoders()
      .AddRequests()
      .AddManagers()
      .AddDelegatingHandlers();

    return services;
  }

  protected IServiceProvider BuildServiceProvider(Action<IServiceCollection>? configure = null)
  {
    var services = ConfigureServices();
    configure?.Invoke(services);
    return services.BuildServiceProvider();
  }
}