﻿using HybridApp.Data;
using IdentityModel.OidcClient;

namespace HybridApp;
public static class MauiProgram
{
  public static MauiApp CreateMauiApp()
  {
    var builder = MauiApp.CreateBuilder();
    builder
      .UseMauiApp<App>()
      .ConfigureFonts(fonts =>
      {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
      });

    builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

    builder.Services.AddSingleton<WeatherForecastService>();

    builder.Services.AddSingleton(new OidcClient(new OidcClientOptions
    {
      Authority = "https://localhost:5000",
      ClientId = "130e78e1-8d0f-4ebc-a62c-7bb219519b1e",
      Scope = "openid profile email phone identityprovider:userinfo weather:read",
      RedirectUri = "hybridapp://callback",
      Browser = new AuthenticationBrowser()
    }));

    return builder.Build();
  }
}