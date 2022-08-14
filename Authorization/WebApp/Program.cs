using AuthorizationServer;
using AuthorizationServer.Extensions;
using Domain;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
{
  loggerConfiguration
  .Enrich.FromLogContext()
  .MinimumLevel.Information()
  .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
  .WriteTo.Console();
});

builder.WebHost.ConfigureServices(services =>
{
  services.AddControllersWithViews();
  services.AddControllers();
  services.AddEndpointsApiExplorer();
  var authConfiguration = builder.Configuration.GetSection("Identity").Get<IdentityConfiguration>();
  services.AddSingleton(authConfiguration);

  var tokenValidationParameters = new TokenValidationParameters
  {
    ValidateAudience = true,
    ValidateIssuer = true,
    ValidateIssuerSigningKey = true,
    IgnoreTrailingSlashWhenValidatingAudience = true,
    ValidIssuer = authConfiguration.ExternalIssuer,
    ValidAudience = authConfiguration.Audience
  };
  services.AddSingleton(tokenValidationParameters);

  services
    .AddAuthentication(configureOptions =>
    {
      configureOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      configureOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddJwtBearer(OpenIdConnectDefaults.AuthenticationScheme, config =>
    {
      config.IncludeErrorDetails = true; //DEVELOP READY
      config.RequireHttpsMetadata = false; //DEVELOP READY
      config.TokenValidationParameters = tokenValidationParameters;
      config.SaveToken = true;
      config.Validate();
    });

  services.AddAuthorization();
  services.AddDatastore(builder.Configuration);
  services.AddCookiePolicy(cookiePolicyOptions =>
  {
    cookiePolicyOptions.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    cookiePolicyOptions.MinimumSameSitePolicy = SameSiteMode.None;
    cookiePolicyOptions.Secure = CookieSecurePolicy.Always;
  });
  services.AddCors(corsOptions =>
  {
    corsOptions.AddDefaultPolicy(corsPolicyBuilder =>
    {
      corsPolicyBuilder
      .AllowAnyOrigin()
      .AllowAnyHeader();
    });
  });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
var identityConfiguration = scope.ServiceProvider.GetRequiredService<IdentityConfiguration>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUserExtended>>();
await identityContext.Database.EnsureDeletedAsync();
await identityContext.Database.EnsureCreatedAsync();

await userManager.CreateAsync(new IdentityUserExtended
{
  Address = "John Doe Street, 51",
  Name = "John WaitForIt Doe",
  Birthdate = DateTime.Now,
  Gender = "Man",
  Locale = "DA-DK",
  FamilyName = "Doe",
  MiddleName = "WaitForIt",
  GivenName = "John",
  NickName = "John",
  UserName = "jokk",
  NormalizedEmail = "HEJMEDDIG@GMAIL.COM",
  NormalizedUserName = "JOKK",
  Email = "hejmeddig@gmail.com",
  PhoneNumber = "88888888"
}, "Password12!");

await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTimeOffset.UtcNow.AddDays(-7));
await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTimeOffset.UtcNow);
await JwkManager.GenerateJwkAsync(identityContext, identityConfiguration, DateTimeOffset.UtcNow.AddDays(7));


if (!app.Environment.IsDevelopment())
{
  IdentityModelEventSource.ShowPII = true;
  app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();