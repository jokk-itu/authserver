﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIDC.Client.Settings;

namespace OIDC.Client.Configure;
public class ConfigureOpenIdConnectOptions : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly IOptions<IdentityProviderSettings> _identityProviderOptions;

    public ConfigureOpenIdConnectOptions(
      IOptions<IdentityProviderSettings> identityProviderOptions)
    {
        _identityProviderOptions = identityProviderOptions;
    }

    public void Configure(OpenIdConnectOptions options)
    {
        options.Authority = _identityProviderOptions.Value.Authority;
        options.TokenValidationParameters.ValidIssuer = _identityProviderOptions.Value.Authority;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.DisableTelemetry = true;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.Prompt = string.Join(' ', _identityProviderOptions.Value.Prompt);
        foreach (var scope in _identityProviderOptions.Value.Scope)
        {
          options.Scope.Add(scope);
        }
        options.ResponseMode = _identityProviderOptions.Value.ResponseMode;
        if (_identityProviderOptions.Value.MaxAge.HasValue)
        {
          options.MaxAge = TimeSpan.FromSeconds(_identityProviderOptions.Value.MaxAge.Value);
        }
        options.SaveTokens = true;

        options.NonceCookie = new CookieBuilder
        {
            Name = $"{_identityProviderOptions.Value.ClientName}-OIDC-Nonce",
            SameSite = SameSiteMode.Strict,
            SecurePolicy = CookieSecurePolicy.Always,
            IsEssential = true,
            HttpOnly = true
        };
        options.CorrelationCookie = new CookieBuilder
        {
            Name = $"{_identityProviderOptions.Value.ClientName}-OIDC-Correlation",
            SameSite = SameSiteMode.Strict,
            SecurePolicy = CookieSecurePolicy.Always,
            IsEssential = true,
            HttpOnly = true
        };
    }

    public void Configure(string name, OpenIdConnectOptions options)
    {
      Configure(options);
    }
}