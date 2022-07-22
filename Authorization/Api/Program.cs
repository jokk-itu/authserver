using AuthorizationServer;
using AuthorizationServer.Extensions;
using AuthorizationServer.TokenFactories;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.IdentityModel.Tokens;

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
  var authConfiguration = builder.Configuration.GetSection("Identity").Get<IdentityConfiguration>();
  services.AddSingleton(authConfiguration);
  services.AddDataProtection();
  services.AddTransient<AuthorizationCodeTokenFactory>();
  services.AddTransient<AccessTokenFactory>();
  services.AddTransient<IdTokenFactory>();
  services.AddTransient<RefreshTokenFactory>();

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

  services.AddAuthentication(OAuthDefaults.DisplayName)
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
});

var app = builder.Build();

var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<IdentityContext>();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
