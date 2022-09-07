using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.IdentityModel.Logging;
using Serilog;
using WebApp;
using WebApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
{
  loggerConfiguration
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Application", "AuthorizationServer")
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Seq(builder.Configuration.GetSection("Log")["SeqUrl"])
    .WriteTo.Console();
});

builder.WebHost.ConfigureServices(services =>
{
  services.AddControllersWithViews();
  services.AddControllers();
  services.AddEndpointsApiExplorer();
  var identityConfiguration = builder.Configuration.GetSection("Identity").Get<IdentityConfiguration>();
  services.AddSingleton(identityConfiguration);

  services.AddOpenIdAuthentication(identityConfiguration);
  services.AddOpenIdAuthorization();

  services.AddDatastore(builder.Configuration);
  services.AddCorsPolicy();
  services.AddCookiePolicy();
  services.AddAntiforgery(antiForgeryOptions =>
  {
    antiForgeryOptions.FormFieldName = "AntiForgeryField";
    antiForgeryOptions.Cookie.Name = "AntiForgeryCookie";
  });
});

var app = builder.Build();

await app.UseTestData();

if (!app.Environment.IsDevelopment())
  app.UseExceptionHandler("/Home/Error");


if(app.Environment.IsDevelopment())
  IdentityModelEventSource.ShowPII = true;

app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }