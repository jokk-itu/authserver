using Application;
using Domain;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Specs;
public abstract class BaseUnitTest
{

  protected readonly IdentityContext IdentityContext;
  protected readonly IServiceProvider ServiceProvider;

  protected BaseUnitTest()
  {
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<IdentityContext>()
      .UseSqlite(connection)
      .Options;
    IdentityContext = new IdentityContext(options);
    IdentityContext.Database.EnsureCreated();

    var services = new ServiceCollection()
      .AddSingleton(_ => new IdentityConfiguration
      {
        AccessTokenExpiration = 3600,
        IdTokenExpiration = 300,
        RefreshTokenExpiration = 26000,
        EncryptingKeySecret = "WnZr4u7w!z%C*F-J",
        CodeSecret = CryptographyHelper.GetRandomString(32),
        PrivateKeySecret = CryptographyHelper.GetRandomString(32),
        Issuer = "auth-server",
      })
      .AddScoped(_ => IdentityContext)
      .AddLogging()
      .AddBuilders()
      .AddDataServices()
      .AddDecoders()
      .AddManagers()
      .AddTransient(x =>
      {
        var userStore = new UserStore<User>(x.GetRequiredService<IdentityContext>());
        return new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
      });
    services.Configure<JwtBearerOptions>(config =>
    {
      config.Audience = "api";
      config.Authority = "auth-server";
    });
    var rootServiceProvider = services.BuildServiceProvider();
    ServiceProvider = rootServiceProvider.CreateScope().ServiceProvider;
  }
}