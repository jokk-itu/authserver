using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OIDC.Client.Settings;

namespace OIDC.Client.Registration;
public class RegistrationService : IRegistrationService
{
  private readonly IOptions<IdentityProviderSettings> _identityProviderSettings;
  private readonly HttpClient _httpClient;

    public RegistrationService(
      IOptions<IdentityProviderSettings> identityProviderSettings,
      IHttpClientFactory httpClientFactory)
    {
      _httpClient = httpClientFactory.CreateClient("IdentityProvider");
      _identityProviderSettings = identityProviderSettings;
    }

    public async Task<RegistrationResponse> Register(OpenIdConnectOptions options, CancellationToken cancellationToken = default)
    {
      if (options.ConfigurationManager is null)
      {
        throw new ArgumentException($"{options.ConfigurationManager} is null");
      }

      var configuration = await options.ConfigurationManager.GetConfigurationAsync(cancellationToken);
      var authority = new Uri(_identityProviderSettings.Value.ClientUri, UriKind.Absolute);
      var backChannelLogOutUri = new Uri(authority, options.RemoteSignOutPath.Value);
      var redirectUri = new Uri(authority, options.CallbackPath);
      var postLogOutRedirectUri = new Uri(authority, options.SignedOutCallbackPath);

      var request = new RegistrationRequest
      {
        ClientName = _identityProviderSettings.Value.ClientName,
        ClientUri = _identityProviderSettings.Value.ClientUri,
        BackChannelLogoutUri = backChannelLogOutUri.AbsoluteUri,
        RedirectUris = new [] { redirectUri.AbsoluteUri },
        PostLogoutRedirectUris = new [] { postLogOutRedirectUri.AbsoluteUri },
        ResponseTypes = new [] { options.ResponseType },
        GrantTypes = _identityProviderSettings.Value.GrantTypes,
        Scope = string.Join(' ', _identityProviderSettings.Value.Scope),
        TokenEndpointAuthMethod = _identityProviderSettings.Value.TokenEndpointAuthMethod
      };
      var responseMessage = await _httpClient.PostAsJsonAsync(configuration.RegistrationEndpoint, request, cancellationToken: cancellationToken);
      responseMessage.EnsureSuccessStatusCode();
      var registrationResponse =
        await responseMessage.Content.ReadFromJsonAsync<RegistrationResponse>(cancellationToken: cancellationToken);

      return registrationResponse;
    }
}
