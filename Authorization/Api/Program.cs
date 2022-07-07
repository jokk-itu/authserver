using AuthorizationServer;
using AuthorizationServer.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureServices(services =>
{
  services.AddControllers();
  services.AddApiVersioning(config => { config.ReportApiVersions = true; });
  services.AddVersionedApiExplorer(config =>
  {
    config.GroupNameFormat = "'v'VVV";
    config.SubstituteApiVersionInUrl = true;
  });
  services.AddEndpointsApiExplorer();
  services.AddSwaggerGen();
  services.AddOptions<ConfigureSwaggerOptions>();
  var authConfiguration = builder.Configuration.GetSection("Identity").Get<AuthenticationConfiguration>();
  services.AddSingleton(authConfiguration);
  services.AddDataProtection();

  var tokenValidationParameters = new TokenValidationParameters
  {
    ValidateAudience = true,
    ValidateIssuer = true,
    ValidateIssuerSigningKey = true,
    IgnoreTrailingSlashWhenValidatingAudience = true,
    ValidIssuer = authConfiguration.Issuer,
    ValidAudience = authConfiguration.Audience
  };
  services.AddSingleton(tokenValidationParameters);

  services.AddAuthentication("OpenId")
      .AddJwtBearer("OpenId", config =>
      {
        config.IncludeErrorDetails = true; //DEVELOP READY
        config.RequireHttpsMetadata = false; //DEVELOP READY
        config.TokenValidationParameters = tokenValidationParameters;
        config.Validate();
      });

  services.AddAuthorization();
  services.AddDatastore(builder.Configuration);
});

var app = builder.Build();

var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<IdentityContext>();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();
//await context.Database.MigrateAsync();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var groupName in provider.ApiVersionDescriptions.Select(v => v.GroupName))
      options.SwaggerEndpoint(
          $"/swagger/{groupName}/swagger.json",
          groupName.ToUpper());
  });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
