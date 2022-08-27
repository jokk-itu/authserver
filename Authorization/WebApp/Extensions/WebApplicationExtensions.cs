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
    await identityContext.Database.EnsureDeletedAsync();
    await identityContext.Database.EnsureCreatedAsync();

    await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTimeOffset.UtcNow.AddDays(-7));
    await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTimeOffset.UtcNow);
    await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTimeOffset.UtcNow.AddDays(7));

    await testManager.AddDataAsync();
    return app;
  }
}
