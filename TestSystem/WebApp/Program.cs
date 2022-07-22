using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApp;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureServices(services => 
{
  services.AddControllersWithViews();
  services.AddAuthentication(configureOptions => 
  {
    configureOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    configureOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    configureOptions.DefaultChallengeScheme = OAuthDefaults.DisplayName;
  })
  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddOAuth(OAuthDefaults.DisplayName, configureOptions => 
  {
    var identity = builder.Configuration.GetSection("Identity");
    configureOptions.ClientId = identity["ClientId"];
    configureOptions.ClientSecret = identity["ClientSecret"];
    configureOptions.AuthorizationEndpoint = $"http://localhost:5000{identity["AuthorizationPath"]}";
    configureOptions.TokenEndpoint = $"http://auth-app:80{identity["TokenPath"]}";
    configureOptions.CallbackPath = identity["CallbackPath"];
    configureOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    configureOptions.UsePkce = true;
    configureOptions.SaveTokens = true;
    configureOptions.Scope.Add("profile");
    configureOptions.Scope.Add("openid");
    configureOptions.Scope.Add("api1");
    configureOptions.CorrelationCookie = new CookieBuilder 
    {
      Name = "Auth-Correlation",
      SameSite = SameSiteMode.None,
      SecurePolicy = CookieSecurePolicy.Always,
      IsEssential = true,
      HttpOnly = true
    };
    configureOptions.Validate();
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
    httpClient.BaseAddress = new Uri("http://webapi:80");
  }).AddHttpMessageHandler<PopulateAccessTokenDelegatingHandler>();

  services.AddHttpContextAccessor();
  services.AddTransient<PopulateAccessTokenDelegatingHandler>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();