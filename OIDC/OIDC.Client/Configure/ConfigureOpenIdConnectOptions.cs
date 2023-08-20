using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIDC.Client.Handlers.Abstract;
using OIDC.Client.Settings;

namespace OIDC.Client.Configure;

public class ConfigureOpenIdConnectOptions : IConfigureNamedOptions<OpenIdConnectOptions>
{
  private readonly IOptionsMonitor<IdentityProviderSettings> _identityProviderOptions;
  private readonly IOpenIdConnectEventHandler _openIdConnectEventHandler;

  public ConfigureOpenIdConnectOptions(
    IOptionsMonitor<IdentityProviderSettings> identityProviderOptions,
    IOpenIdConnectEventHandler openIdConnectEventHandler)
  {
    _identityProviderOptions = identityProviderOptions;
    _openIdConnectEventHandler = openIdConnectEventHandler;
  }

  public void Configure(OpenIdConnectOptions options)
  {
    options.Authority = _identityProviderOptions.CurrentValue.Authority;
    options.TokenValidationParameters.ValidIssuer = _identityProviderOptions.CurrentValue.Authority;
    options.TokenValidationParameters.ValidAudience = _identityProviderOptions.CurrentValue.ClientId;
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";
    options.GetClaimsFromUserInfoEndpoint = true;
    options.DisableTelemetry = true;
    options.ResponseType = OpenIdConnectResponseType.Code;

    options.ClaimActions.MapUniqueJsonKey("auth_time", "auth_time");
    options.ClaimActions.MapUniqueJsonKey("grant_id", "grant_id");
    options.ClaimActions.MapUniqueJsonKey("address", "address");
    options.ClaimActions.MapUniqueJsonKey("given_name", "given_name");
    options.ClaimActions.MapUniqueJsonKey("family_name", "family_name");
    options.ClaimActions.MapUniqueJsonKey("birthdate", "birthdate");
    options.ClaimActions.MapUniqueJsonKey("name", "name");
    options.ClaimActions.MapUniqueJsonKey("email", "email");
    options.ClaimActions.MapUniqueJsonKey("phone", "phone");
    options.ClaimActions.MapUniqueJsonKey("locale", "locale");

    options.ClientId = _identityProviderOptions.CurrentValue.ClientId;
    options.ClientSecret = _identityProviderOptions.CurrentValue.ClientSecret;

    foreach (var scope in _identityProviderOptions.CurrentValue.Scope)
    {
      options.Scope.Add(scope);
    }

    options.ResponseMode = _identityProviderOptions.CurrentValue.ResponseMode;
    if (_identityProviderOptions.CurrentValue.MaxAge.HasValue)
    {
      options.MaxAge = TimeSpan.FromSeconds(_identityProviderOptions.CurrentValue.MaxAge.Value);
    }

    options.SaveTokens = true;

    options.NonceCookie = new CookieBuilder
    {
      Name = $"{_identityProviderOptions.CurrentValue.ClientName}-OIDC-Nonce",
      SameSite = SameSiteMode.Strict,
      SecurePolicy = CookieSecurePolicy.Always,
      IsEssential = true,
      HttpOnly = true
    };
    options.CorrelationCookie = new CookieBuilder
    {
      Name = $"{_identityProviderOptions.CurrentValue.ClientName}-OIDC-Correlation",
      SameSite = SameSiteMode.Strict,
      SecurePolicy = CookieSecurePolicy.Always,
      IsEssential = true,
      HttpOnly = true
    };

    options.Events = new OpenIdConnectEvents
    {
      OnRedirectToIdentityProviderForSignOut = _openIdConnectEventHandler.SetClientIdOnRedirect
    };
  }

  public void Configure(string name, OpenIdConnectOptions options)
  {
    Configure(options);
  }
}