using System.Net.Http.Headers;
using System.Text;
using AuthServer.Core.Models;
using AuthServer.Enums;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Extensions;
public static class HttpRequestExtensions
{
    public static ClientAuthentication? GetClientSecretBasic(this HttpRequest request)
    {
        if (!AuthenticationHeaderValue.TryParse(request.Headers.Authorization, out var authenticationHeader)
            || authenticationHeader.Scheme != "Basic"
            || string.IsNullOrWhiteSpace(authenticationHeader.Parameter))
        {
            return null;
        }

        var decodedBytes = Convert.FromBase64String(authenticationHeader.Parameter);
        var decoded = Encoding.UTF8.GetString(decodedBytes).Split(":");

        var clientId = decoded.ElementAtOrDefault(0);
        var clientSecret = decoded.ElementAtOrDefault(1);

        var isClientSecretBasic = !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);

        return isClientSecretBasic
            ? new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, clientId!, clientSecret!)
            : null;
    }
}