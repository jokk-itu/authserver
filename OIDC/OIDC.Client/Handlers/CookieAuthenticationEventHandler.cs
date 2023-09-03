using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIDC.Client.Handlers.Abstract;

namespace OIDC.Client.Handlers;

public class CookieAuthenticationEventHandler : ICookieAuthenticationEventHandler
{
  private readonly IOptions<OpenIdConnectOptions> _openIdConnectOptions;
  private readonly ILogger<CookieAuthenticationEventHandler> _logger;
  private readonly HttpClient _httpClient;

  public CookieAuthenticationEventHandler(
    IOptions<OpenIdConnectOptions> openIdConnectOptions,
    ILogger<CookieAuthenticationEventHandler> logger,
    IHttpClientFactory httpClientFactory)
  {
    _openIdConnectOptions = openIdConnectOptions;
    _logger = logger;
    _httpClient = httpClientFactory.CreateClient("IdentityProvider");
  }

  public async Task GetRefreshTokenIfExceededExpiration(CookieValidatePrincipalContext context)
  {
    if (context.Principal?.Identity?.IsAuthenticated == false)
    {
      _logger.LogDebug("User is not authenticated");
      return;
    }

    var tokens = context.Properties.GetTokens().ToList();
    var expiresIn = tokens.Find(t => t.Name == "expires_at")?.Value ?? "0";
    var expiration = DateTime.Parse(expiresIn).ToUniversalTime();
    if (expiration > DateTime.UtcNow)
    {
      _logger.LogDebug("Token has not expired {expiresAt}", expiration);
      return;
    }

    var configuration =
      await _openIdConnectOptions.Value.ConfigurationManager!.GetConfigurationAsync(CancellationToken.None);

    var tokenClientOptions = new TokenClientOptions
    {
      ClientCredentialStyle = ClientCredentialStyle.PostBody,
      ClientId = _openIdConnectOptions.Value.ClientId!,
      ClientSecret = _openIdConnectOptions.Value.ClientSecret,
      Address = configuration.TokenEndpoint
    };

    var tokenClient = new TokenClient(_httpClient, tokenClientOptions);
    var tokenResponse = await tokenClient.RequestRefreshTokenAsync(OpenIdConnectGrantTypes.RefreshToken);
    if (tokenResponse.IsError)
    {
      _logger.LogError(tokenResponse.Exception,
        "Error occurred during refresh token request. Error {ErrorCode}. ErrorDescription {ErrorDescription}. StatusCode {StatusCode}",
        tokenResponse.Error,
        tokenResponse.ErrorDescription,
        tokenResponse.HttpStatusCode);

      context.RejectPrincipal();
      return;
    }

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
        Name = "id_token",
        Value = tokenResponse.IdentityToken
      }
    });
  }
}