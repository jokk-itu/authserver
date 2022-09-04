using Infrastructure.Repositories;
using Infrastructure;

namespace WebApp.Extensions;

public static class WebApplicationExtensions
{
  public static async Task<WebApplication> UseTestData(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
    var identityConfiguration = scope.ServiceProvider.GetRequiredService<IdentityConfiguration>();
    var testManager = scope.ServiceProvider.GetRequiredService<TestManager>();

    if (await identityContext.Database.CanConnectAsync())
      return app;

    await identityContext.Database.EnsureCreatedAsync();

    await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTime.UtcNow.AddDays(-7));
    await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTime.UtcNow);
    await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTime.UtcNow.AddDays(7));

    await testManager.AddDataAsync();
    return app;
  }
}
