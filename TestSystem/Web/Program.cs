using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp;
using WebApp.Services;
using Serilog;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication;
using IdentityModel.Client;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggingConfiguration) => 
{
  loggingConfiguration
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "WebApp")
    .WriteTo.Seq(builder.Configuration.GetSection("Log")["SeqUrl"])
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
  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, configureOptions =>
  {
    configureOptions.Events = new CookieAuthenticationEvents
    {
      OnValidatePrincipal = async context =>
      {
        if (context.Principal?.Identity?.IsAuthenticated ?? false)
        {
          var tokens = context.Properties.GetTokens();
          var expires = DateTime.Parse(tokens.FirstOrDefault(token => token.Name == "expires_at")!.Value).ToUniversalTime();
          if (expires < DateTime.UtcNow)
          {
            var openIdConnectOptions = context.HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<OpenIdConnectOptions>>().Get(OpenIdConnectDefaults.AuthenticationScheme);
            var tokenClientOptions = new TokenClientOptions
            {
              ClientCredentialStyle = ClientCredentialStyle.PostBody,
              ClientId = openIdConnectOptions.ClientId,
              ClientSecret = openIdConnectOptions.ClientSecret
            };
            using var httpClient = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
            var tokenClient = new TokenClient(httpClient, tokenClientOptions);
            var tokenResponse = await tokenClient.RequestRefreshTokenAsync(OpenIdConnectGrantTypes.RefreshToken);
            if (tokenResponse.IsError)
            {
              context.RejectPrincipal();
              return;
            }
            var expirationValue = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            context.Properties.StoreTokens(new[]
            {
              new AuthenticationToken
              {
                Name = "access_token",
                Value = tokenResponse.AccessToken
              },
              new AuthenticationToken
              {
                Name = "refresh_token",
                Value = tokenResponse.RefreshToken
              },
              new AuthenticationToken
              {
                Name = "expires_at",
                Value = expirationValue.ToString()
              }
            });
            context.ShouldRenew = true;
          }
        }
      },
    };
  })
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
      OnTokenValidated = context => 
      {
        Log.Information("IdToken Validated");
        return Task.CompletedTask;
      },
      OnTokenResponseReceived = context => 
      {
        Log.Information("Received Token Response");
        return Task.CompletedTask;
      },
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
    httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("WeatherService")["Url"]);
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