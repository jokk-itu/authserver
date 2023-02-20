using System.Security.Claims;
using IdentityModel.OidcClient;
using Microsoft.AspNetCore.Components.Authorization;

namespace HybridApp;
public class MauiAuthenticationStateProvider : AuthenticationStateProvider
{
  private readonly OidcClient _client;

  public MauiAuthenticationStateProvider(OidcClient client)
  {
    _client = client;
  }

  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    // TODO save access_token, refresh_token and id_token in Secure Storage
    // TODO if secure storage APIs defers between systems, create an interface
    // TODO and only create the implementation for Windows

    // TODO when calling this function, get the id_token, if it fresh, construct state with it
    // TODO if it is not, call refresh_token function in OIDC client

    // TODO create service for WeatherService to call api and get the access_token from secure storage
    var result = await _client.LoginAsync();
    if (result.IsError)
    {
      return new AuthenticationState(new ClaimsPrincipal());
    }

    return new AuthenticationState(result.User);
  }
}
