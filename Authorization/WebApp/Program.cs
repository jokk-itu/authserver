using AuthorizationServer;
using AuthorizationServer.Extensions;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
    .AddJwtBearer(OAuthDefaults.DisplayName, config =>
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
    var policy = new CorsPolicy();
    policy.Origins.Add("localhost:5002");
    policy.SupportsCredentials = true;
    corsOptions.AddDefaultPolicy(policy);
  });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<IdentityContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
var jwkManager = scope.ServiceProvider.GetRequiredService<JwkManager>();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

await userManager.CreateAsync(new IdentityUser
{
  UserName = "jokk",
  NormalizedUserName = "JOKK",
  Email = "hejmeddig@gmail.com",
  NormalizedEmail = "HEJMEDDIG@GMAIL.COM",
  PhoneNumber = "88888888"
}, "Password12!");

await jwkManager.GenerateJwkAsync();


if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();