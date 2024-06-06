using AuthServer.Core;
using AuthServer.Core.Models;
using AuthServer.Enums;
using AuthServer.TokenDecoders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AuthServer.Extensions;

public static class FormCollectionExtensions
{
    public static ClientAuthentication? GetClientSecretPost(this IFormCollection formCollection)
    {
        formCollection.TryGetValue(Parameter.ClientId, out var clientId);
        formCollection.TryGetValue(Parameter.ClientSecret, out var clientSecret);

        var isClientSecretPost = !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);
        return isClientSecretPost
            ? new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretPost, clientId!, clientSecret!)
            : null;
    }

    public static ClientAuthentication? GetClientId(this IFormCollection formCollection)
    {
        formCollection.TryGetValue(Parameter.ClientId, out var clientId);
        formCollection.TryGetValue(Parameter.ClientSecret, out var clientSecret);

        var isClientId = !string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(clientSecret);
        return isClientId
            ? new ClientIdAuthentication(clientId!)
            : null;
    }

    public static ClientAuthentication? GetClientAssertion(this IFormCollection formCollection, ClientTokenAudience clientTokenAudience)
    {
        formCollection.TryGetValue(Parameter.ClientId, out var clientId);
        formCollection.TryGetValue(Parameter.ClientAssertion, out var clientAssertion);
        formCollection.TryGetValue(Parameter.ClientAssertionType, out var clientAssertionType);

        var isClientAssertion = !string.IsNullOrWhiteSpace(clientAssertion)
                                && !string.IsNullOrWhiteSpace(clientAssertionType);

        return isClientAssertion
            ? new ClientAssertionAuthentication(clientTokenAudience, clientId, clientAssertionType!, clientAssertion!)
            : null;
    }

    public static string GetValueOrEmpty(this IFormCollection formCollection, string key)
    {
        formCollection.TryGetValue(key, out var value);
        return value == StringValues.Empty ? string.Empty : value.ToString();
    }

    public static IReadOnlyCollection<string> GetSpaceDelimitedValueOrEmpty(this IFormCollection formCollection, string key)
    {
	    formCollection.TryGetValue(key, out var value);
	    var hasValue = !StringValues.IsNullOrEmpty(value);
	    return !hasValue ? [] : value.ToString().Split(' ');
	}
}