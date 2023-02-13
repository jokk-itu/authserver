using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Transforms;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
{
  loggerConfiguration
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Application", "Wasm")
    .WriteTo.Console();
});

builder.WebHost.ConfigureServices((context, services) =>
{
  var identityConfiguration = context.Configuration.GetSection("Identity");
  services.AddControllersWithViews();
  services.AddRazorPages();
  services
    .AddAuthentication(options =>
    {
      options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(x =>
    {
      x.LoginPath = "/api/login";
      x.LogoutPath = "/api/logout";
      x.ReturnUrlParameter = "/";
    })
    .AddOpenIdConnect(options =>
    {
      options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.Authority = identityConfiguration["Authority"];
      options.ClientId = identityConfiguration["ClientId"];
      options.ClientSecret = identityConfiguration["ClientSecret"];
      options.MetadataAddress = $"{identityConfiguration["Authority"]}{identityConfiguration["MetaPath"]}";
      options.CallbackPath = identityConfiguration["CallbackPath"];
      options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.ResponseType = OpenIdConnectResponseType.Code;
      options.UsePkce = true;
      options.SaveTokens = true;
      options.Prompt = "login consent";
      options.Scope.Add("profile");
      options.Scope.Add("email");
      options.Scope.Add("phone");
      options.Scope.Add("openid");
      options.Scope.Add("weather:read");
      options.Scope.Add("identityprovider:read");
      options.MapInboundClaims = true;
      options.GetClaimsFromUserInfoEndpoint = true;
    });

  services.AddAuthorization();

  services.AddAuthorization(options => options.AddPolicy("CookieAuthenticationPolicy", policyBuilder =>
  {
    policyBuilder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    policyBuilder.RequireAuthenticatedUser();
  }));

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