using System.Net;
using System.Text.Json;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;
public class UserInfoEndpointBuilder
{
  private string _accessToken = string.Empty;

  public static UserInfoEndpointBuilder Instance()
  {
    return new UserInfoEndpointBuilder();
  }

  public UserInfoEndpointBuilder AddAccessToken(string accessToken)
  {
    _accessToken = accessToken;
    return this;
  }

  public async Task<IDictionary<string, string>> BuildUserInfo(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "connect/userinfo");
    userInfoRequest.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"Bearer {_accessToken}");
    var userInfoResponse = await httpClient.SendAsync(userInfoRequest, cancellationToken);
    userInfoResponse.EnsureSuccessStatusCode();
    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync(cancellationToken);
    return JsonSerializer.Deserialize<Dictionary<string, string>>(userInfoContent)!;
  }
}
