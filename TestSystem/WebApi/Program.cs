using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
{
  loggerConfiguration
  .Enrich.FromLogContext()
  .WriteTo.Console();
});

builder.WebHost.ConfigureServices(services =>
{
  IdentityModelEventSource.ShowPII = true; //DEVELOPER ready
  services.AddControllers();
  services.AddEndpointsApiExplorer();
  services.AddSwaggerGen();
  services
  .AddAuthentication(authenticationOptions =>
  {
    authenticationOptions.DefaultScheme = OpenIdConnectDefaults.AuthenticationScheme;
    authenticationOptions.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
  })
  .AddJwtBearer(OpenIdConnectDefaults.AuthenticationScheme, jwtBearerOptions =>
  {
    var identity = builder.Configuration.GetSection("Identity");
    jwtBearerOptions.Authority = identity["Authority"];
    jwtBearerOptions.Audience = identity["Audience"];
    jwtBearerOptions.RequireHttpsMetadata = false;
    jwtBearerOptions.Challenge = OAuthDefaults.DisplayName;
    jwtBearerOptions.MetadataAddress = $"{jwtBearerOptions.Authority}/.well-known/openid-configuration";
    jwtBearerOptions.Events = new JwtBearerEvents
    {
      OnAuthenticationFailed = context =>
      {
        Log.Error(context.Exception, "Error occured during authentication");
        return Task.CompletedTask;
      },
      OnTokenValidated = context =>
      {
        Log.Information("User is validated");
        return Task.CompletedTask;
      },
      OnForbidden = context =>
      {
        Log.Error("User does not have authorization");
        return Task.CompletedTask;
      }
    };
    jwtBearerOptions.IncludeErrorDetails = true;
    jwtBearerOptions.SaveToken = true;
    jwtBearerOptions.Validate();
  });
  services.AddAuthorization();
});

Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console().CreateBootstrapLogger();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();