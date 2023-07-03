using System.Net.Http.Headers;
using Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
{
  loggerConfiguration
    .Enrich.FromLogContext()
    .MinimumLevel.Warning()
    .Enrich.WithProperty("Application", "WeatherService")
    .MinimumLevel.Override("Api", LogEventLevel.Information)
    .WriteTo.Console();
});

builder.WebHost.ConfigureServices(services =>
{
  services.AddControllers();
  services.AddEndpointsApiExplorer();
  services.AddSwaggerGen();
  services.AddHttpClient("IdP", (serviceProvider, client) =>
  {
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var baseAddress = config.GetSection("Identity")["Authority"];
    client.BaseAddress = new Uri(baseAddress);
  });
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
    jwtBearerOptions.Challenge = OpenIdConnectDefaults.AuthenticationScheme;
    jwtBearerOptions.MetadataAddress = $"{jwtBearerOptions.Authority}/.well-known/openid-configuration";
    jwtBearerOptions.Events = new JwtBearerEvents
    {
      OnAuthenticationFailed = context =>
      {
        Log.Error(context.Exception, "Error occurred during authentication");
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
      },
      OnMessageReceived = async context =>
      {
        Log.Information("Initiating bearer validation");
        var isStructuredToken = context.Token?.Split('.').Length is 3 or 5;
        if (!string.IsNullOrWhiteSpace(context.Token) && !isStructuredToken)
        {
          var configuration = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()
            .GetSection("Identity");

          var httpClientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
          var httpClient = httpClientFactory.CreateClient("IdP");
          var request = new HttpRequestMessage();
          request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.Token);
          var content = new Dictionary<string, object>
          {
            {"client_id", configuration["ClientId"]},
            {"client_secret", configuration["ClientSecret"]},
            {"token", context.Token}
          };
          var response = await httpClient.PostAsJsonAsync("connect/token/introspection",
            content, context.HttpContext.RequestAborted);

          response.EnsureSuccessStatusCode();
          var token = await response.Content.ReadFromJsonAsync<IntrospectionResponse>();
          if (token?.Active ?? false)
          {
            context.Success();
          }
          else
          {
            context.Fail("token is not active");
          }
        }
      },
    };
    jwtBearerOptions.IncludeErrorDetails = true;
    jwtBearerOptions.SaveToken = true;
    jwtBearerOptions.Validate();
  });
  services.AddAuthorization(options =>
  {
    options.AddPolicy("Weather", policyBuilder =>
    {
      policyBuilder.RequireAssertion(authorizationContext =>
      {
        var scope = authorizationContext.User.Claims.SingleOrDefault(x => x.Type == "scope");
        return scope is not null && scope.Value.Contains("weather:read");
      });
    });
  });

  services.Configure<ForwardedHeadersOptions>(options =>
  {
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
  });
});

Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console().CreateBootstrapLogger();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
  IdentityModelEventSource.ShowPII = true;
}

app.UseForwardedHeaders();
app.UseHsts();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();