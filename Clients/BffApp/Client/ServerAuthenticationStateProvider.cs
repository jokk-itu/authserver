using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Shared;

namespace Client;

public class ServerAuthenticationStateProvider : AuthenticationStateProvider
{
  private readonly HttpClient _httpClient;

  public ServerAuthenticationStateProvider(IHttpClientFactory httpClientFactory)
  {
    _httpClient = httpClientFactory.CreateClient("Server");
  }

  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    try
    {
      var userDto = await _httpClient.GetFromJsonAsync<UserDto>("api/user");
      if (userDto is null)
      {
        return new AuthenticationState(new ClaimsPrincipal());
      }

      var identity = new ClaimsIdentity(
        userDto.Claims.Select(x => new Claim(x.Type, x.Value)),
        "serverAuth", "name", "role");

      return new AuthenticationState(new ClaimsPrincipal(identity));
    }
    catch (Exception)
    {
      return new AuthenticationState(new ClaimsPrincipal());
    }
  }
}