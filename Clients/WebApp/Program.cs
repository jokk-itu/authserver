using App;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Serilog;
using Microsoft.IdentityModel.Logging;
using App.Services;
using Microsoft.Extensions.Options;
using OIDC.Client.Configure;
using OIDC.Client.Handlers;
using OIDC.Client.Handlers.Abstract;
using OIDC.Client.Registration;
using OIDC.Client.Settings;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggingConfiguration) => 
{
  loggingConfiguration
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "WebApp")
    .MinimumLevel.Information()
    .MinimumLevel.Override("App", LogEventLevel.Information)
    .WriteTo.Console();
});

builder.WebHost.ConfigureServices(services =>
{
  services.AddControllersWithViews();

  services.AddOptions();
  services.AddSingleton<IConfigureOptions<IdentityProviderSettings>, ConfigureIdentityProviderSettings>();
  services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureOpenIdConnectOptions>();
  services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();

  services.AddTransient<ICookieAuthenticationEventHandler, CookieAuthenticationEventHandler>();
  services.AddTransient<IOpenIdConnectEventHandler, OpenIdConnectEventHandler>();
  services.AddTransient<IRegistrationService, RegistrationService>();

  services.AddAuthentication(configureOptions => 
  {
    configureOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    configureOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
  })
  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddOpenIdConnect();

  services.AddSingleton<IPostConfigureOptions<OpenIdConnectOptions>, PostConfigureOpenIdConnectOptions>();

  services.AddAuthorization();
  services.AddCookiePolicy(cookiePolicyOptions =>
  {
    cookiePolicyOptions.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    cookiePolicyOptions.MinimumSameSitePolicy = SameSiteMode.Strict;
    cookiePolicyOptions.Secure = CookieSecurePolicy.Always;
  });
  services.AddHttpClient<WeatherService>(httpClient =>
  {
    httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("WeatherService")["Url"]);
  }).AddHttpMessageHandler<PopulateAccessTokenDelegatingHandler>();

  services.AddHttpClient("IdentityProvider", httpClient =>
  {
    httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("Identity")["Authority"]);
  });

  services.AddHttpContextAccessor();
  services.AddTransient<PopulateAccessTokenDelegatingHandler>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
}

if (app.Environment.IsDevelopment())
{
  IdentityModelEventSource.ShowPII = true;
}

app.UseHttpsRedirection();
app.UseHsts();
app.UseSerilogRequestLogging();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();