using App;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Serilog;
using Microsoft.IdentityModel.Logging;
using App.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using OIDC.Client.Configure;
using OIDC.Client.Handlers;
using OIDC.Client.Handlers.Abstract;
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

builder.WebHost.ConfigureServices((builderContext, services) =>
{
  services.AddControllersWithViews();

  services.AddOptions();
  services.AddSingleton<IConfigureOptions<IdentityProviderSettings>, ConfigureIdentityProviderSettings>();
  services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureOpenIdConnectOptions>();
  services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();

  services.AddTransient<ICookieAuthenticationEventHandler, CookieAuthenticationEventHandler>();
  services.AddTransient<IOpenIdConnectEventHandler, OpenIdConnectEventHandler>();

  services.AddAuthentication(configureOptions => 
  {
    configureOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    configureOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
  })
  .AddCookie(options =>
  {
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
    options.ReturnUrlParameter = "Home";
    options.Cookie.Name = "IdentityCookie-WebApp";
  })
  .AddOpenIdConnect();

  services.AddAuthorization();
  services.AddCookiePolicy(cookiePolicyOptions =>
  {
    cookiePolicyOptions.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    cookiePolicyOptions.MinimumSameSitePolicy = SameSiteMode.Strict;
    cookiePolicyOptions.Secure = CookieSecurePolicy.Always;
  });
  services.AddHttpClient<WeatherService>(httpClient =>
  {
    httpClient.BaseAddress = new Uri(builderContext.Configuration.GetSection("WeatherService")["Url"]);
  }).AddHttpMessageHandler<PopulateAccessTokenDelegatingHandler>();

  services.AddHttpClient("IdentityProvider", httpClient =>
  {
    httpClient.BaseAddress = new Uri(builderContext.Configuration.GetSection("Identity")["Authority"]);
  });

  services.AddHttpContextAccessor();
  services.AddTransient<PopulateAccessTokenDelegatingHandler>();

  builder.Services.Configure<ForwardedHeadersOptions>(options =>
  {
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
  });
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

app.UseForwardedHeaders();
app.UseHsts();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();