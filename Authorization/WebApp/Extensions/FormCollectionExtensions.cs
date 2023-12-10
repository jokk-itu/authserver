using Domain.Enums;
using Infrastructure.Requests.Abstract;
using WebApp.Constants;

namespace WebApp.Extensions;

public static class FormCollectionExtensions
{
  public static ClientAuthentication? GetClientSecretPost(this IFormCollection formCollection)
  {
    var clientAuthentication = new ClientAuthentication
    {
      Method = TokenEndpointAuthMethod.ClientSecretPost
    };

    if (formCollection.TryGetValue(ParameterNames.ClientId, out var clientId))
    {
      clientAuthentication.ClientId = clientId;
    }

    if (formCollection.TryGetValue(ParameterNames.ClientSecret, out var clientSecret))
    {
      clientAuthentication.ClientSecret = clientSecret;
    }

    var isClientSecretPost = !string.IsNullOrWhiteSpace(clientId) || !string.IsNullOrWhiteSpace(clientSecret);
    return isClientSecretPost ? clientAuthentication : null;
  }
}