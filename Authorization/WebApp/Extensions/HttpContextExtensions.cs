using Domain.Enums;
using Infrastructure.Requests.Abstract;
using System.Net.Http.Headers;
using System.Text;

namespace WebApp.Extensions;

public static class HttpContextExtensions
{
  public static ClientAuthentication? GetClientSecretBasic(this HttpContext context)
  {
    if (!AuthenticationHeaderValue.TryParse(context.Request.Headers.Authorization, out var authenticationHeader)
        || authenticationHeader.Scheme != "Basic"
        || string.IsNullOrWhiteSpace(authenticationHeader.Parameter))
    {
      return null;
    }

    var decodedBytes = Convert.FromBase64String(authenticationHeader.Parameter);
    var decoded = Encoding.UTF8.GetString(decodedBytes).Split(":");

    var clientId = decoded.ElementAtOrDefault(0);
    var clientSecret = decoded.ElementAtOrDefault(1);

    var clientAuthentication = new ClientAuthentication
    {
        Method = TokenEndpointAuthMethod.ClientSecretBasic,
        ClientId = clientId,
        ClientSecret = clientSecret
    };

    var isClientSecretBasic = !string.IsNullOrWhiteSpace(clientId) || !string.IsNullOrWhiteSpace(clientSecret);
    return isClientSecretBasic ? clientAuthentication : null;
  }
}