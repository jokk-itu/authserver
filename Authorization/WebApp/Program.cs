using Application;
using Infrastructure.Extensions;
using Microsoft.IdentityModel.Logging;
using Serilog;
using WebApp.Constants;
using WebApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
{
  loggerConfiguration
    .Enrich.FromLogContext()
    .MinimumLevel.Warning()
    .Enrich.WithProperty("Application", "AuthorizationServer")
    .MinimumLevel.Override("WebApp", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Infrastructure", Serilog.Events.LogEventLevel.Information)
    .WriteTo.Console();
});

builder.WebHost.ConfigureServices(services =>
{
  services.AddControllersWithViews();
  services.AddControllers();
  services.AddEndpointsApiExplorer();
  var identityConfiguration = builder.Configuration.GetSection("Identity").Get<IdentityConfiguration>();
  services.AddSingleton(identityConfiguration);

  services.AddOpenIdAuthentication();
  services.AddOpenIdAuthorization();

  services
    .AddDataStore(builder.Configuration)
    .AddBuilders()
    .AddDataServices()
    .AddDecoders()
    .AddManagers()
    .AddRequests()
    .AddContextAccessors()
    .AddDelegatingHandlers();
  
  services.AddCorsPolicy();
  services.AddCookiePolicy();
  services.AddAntiforgery(antiForgeryOptions =>
  {
    antiForgeryOptions.FormFieldName = AntiForgeryConstants.AntiForgeryField;
    antiForgeryOptions.Cookie.Name = AntiForgeryConstants.AntiForgeryCookie;
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

app.UseHsts();
app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program 
{
  public Program()
  {

  }
}