using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy.Transforms;
using Serilog;
using Microsoft.Extensions.Options;
using OIDC.Client.Configure;
using OIDC.Client.Handlers;
using OIDC.Client.Handlers.Abstract;
using OIDC.Client.Settings;
using Serilog.Events;
using Server;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
{
  loggerConfiguration
    .Enrich.FromLogContext()
    .MinimumLevel.Warning()
    .Enrich.WithProperty("Application", "Wasm")
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication.OpenIdConnect", LogEventLevel.Verbose)
    .MinimumLevel.Override("Serilog.AspNetCore", LogEventLevel.Information)
    .MinimumLevel.Override("OIDC.Client", LogEventLevel.Information)
    .WriteTo.Console();
});

builder.WebHost.ConfigureServices((context, services) =>
{
  services.AddControllersWithViews();
  services.AddRazorPages();
  services.AddOptions();
  services.Configure<ForwardedHeadersOptions>(options =>
  {
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
  });

  services.AddSingleton<IConfigureOptions<IdentityProviderSettings>, ConfigureIdentityProviderSettings>();
  services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureOpenIdConnectOptions>();
  services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();
  services.AddTransient<ICookieAuthenticationEventHandler, CookieAuthenticationEventHandler>();
  services
    .AddAuthentication(options =>
    {
      options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(x =>
    {
      x.LoginPath = "/api/user/login";
      x.LogoutPath = "/api/user/logout";
      x.ReturnUrlParameter = "/";
      x.Cookie.Name = "IdentityCookie-Wasm";
    })
    .AddOpenIdConnect();

  services.AddAuthorization(options => options.AddPolicy("CookieAuthenticationPolicy", policyBuilder =>
  {
    policyBuilder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    policyBuilder.RequireAuthenticatedUser();
  }));

  services.AddHttpClient("IdentityProvider", httpClient =>
  {
    httpClient.BaseAddress = new Uri(context.Configuration.GetSection("Identity")["Authority"]);
  });

  services
    .AddReverseProxy()
    .LoadFromConfig(context.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilder => transformBuilder.AddRequestTransform(async requestContext =>
    {
      var token = await requestContext.HttpContext.GetTokenAsync("access_token");
      requestContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseWebAssemblyDebugging();
}
else
{
  app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseHsts();
app.UseSerilogRequestLogging();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.MapReverseProxy();
app.MapFallbackToFile("index.html");

app.Run();