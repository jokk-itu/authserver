using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp;
using WebApp.Services;
using Serilog;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggingConfiguration) => 
{
  loggingConfiguration
    .Enrich.FromLogContext()
    .WriteTo.Console();
});

builder.WebHost.ConfigureServices(services =>
{
  services.AddControllersWithViews();
  services.AddAuthentication(configureOptions =>
  {
    configureOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    configureOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    configureOptions.DefaultChallengeScheme = OpenIdConnectDefaults.DisplayName;
  })
  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, configureOptions =>
  {
    var identity = builder.Configuration.GetSection("Identity");
    configureOptions.Authority = identity["InternalAuthority"];
    configureOptions.ClientId = identity["ClientId"];
    configureOptions.ClientSecret = identity["ClientSecret"];
    configureOptions.MetadataAddress = $"{identity["InternalAuthority"]}{identity["MetaPath"]}";
    configureOptions.CallbackPath = identity["CallbackPath"];
    configureOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    configureOptions.ResponseType = OpenIdConnectResponseType.Code;
    configureOptions.UsePkce = true;
    configureOptions.SaveTokens = true;
    configureOptions.Scope.Add("profile");
    configureOptions.Scope.Add("openid");
    configureOptions.Scope.Add("api1");
    configureOptions.Scope.Add("identity-provider");
    configureOptions.MapInboundClaims = true;
    configureOptions.GetClaimsFromUserInfoEndpoint = true;
    configureOptions.Events = new OpenIdConnectEvents
    {
      OnAccessDenied = context =>
      {
        Log.Information("Access denied");
        return Task.CompletedTask;
      },
      OnAuthorizationCodeReceived = context =>
      {
        Log.Information("AuthorizationCode received");
        return Task.CompletedTask;
      },
      OnRedirectToIdentityProvider = context =>
      {
        Log.Information("Redirecting to Authorize endpoint");
        return Task.CompletedTask;
      },
      OnAuthenticationFailed = context =>
      {
        Log.Information("Authentication failed");
        return Task.CompletedTask;
      }
    };
    configureOptions.RequireHttpsMetadata = false;
    configureOptions.NonceCookie = new CookieBuilder
    {
      Name = "OpenId-Auth-Nonce",
      SameSite = SameSiteMode.None,
      SecurePolicy = CookieSecurePolicy.Always,
      IsEssential = true,
      HttpOnly = true
    };
    configureOptions.CorrelationCookie = new CookieBuilder
    {
      Name = "OpenId-Auth-Correlation",
      SameSite = SameSiteMode.None,
      SecurePolicy = CookieSecurePolicy.Always,
      IsEssential = true,
      HttpOnly = true
    };
  });

  services.AddAuthorization();
  services.AddCookiePolicy(cookiePolicyOptions =>
  {
    cookiePolicyOptions.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    cookiePolicyOptions.MinimumSameSitePolicy = SameSiteMode.None;
    cookiePolicyOptions.Secure = CookieSecurePolicy.Always;
  });
  services.AddHttpClient<WebApiService>(httpClient =>
  {
    httpClient.BaseAddress = new Uri("http://webapi:80");
  }).AddHttpMessageHandler<PopulateAccessTokenDelegatingHandler>();

  services.AddHttpContextAccessor();
  services.AddTransient<PopulateAccessTokenDelegatingHandler>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
  app.UseExceptionHandler("/Home/Error");

if (app.Environment.IsDevelopment())
  IdentityModelEventSource.ShowPII = true;

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();