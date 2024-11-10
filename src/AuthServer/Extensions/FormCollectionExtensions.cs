using AuthServer.Authentication.Models;
using AuthServer.Core;
using AuthServer.Enums;
using AuthServer.TokenDecoders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AuthServer.Extensions;

internal static class FormCollectionExtensions
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
        formCollection.TryGetValue(Parameter.ClientAssertion, out var clientAssertion);
        formCollection.TryGetValue(Parameter.ClientAssertionType, out var clientAssertionType);

        var hasSecretOrAssertion = !string.IsNullOrEmpty(clientSecret)
            || !string.IsNullOrEmpty(clientAssertion)
            || !string.IsNullOrEmpty(clientAssertionType);

        var isClientId = !string.IsNullOrWhiteSpace(clientId) && !hasSecretOrAssertion;
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

    public static string? GetValue(this IFormCollection formCollection, string key)
    {
        formCollection.TryGetValue(key, out var value);
        return value == StringValues.Empty ? null : value.ToString();
    }

    public static IReadOnlyCollection<string> GetSpaceDelimitedValue(this IFormCollection formCollection, string key)
    {
	    formCollection.TryGetValue(key, out var value);
	    var hasValue = !StringValues.IsNullOrEmpty(value);
	    return !hasValue ? [] : value.ToString().Split(' ');
	}

    public static IReadOnlyCollection<string> GetCollectionValue(this IFormCollection formCollection, string key)
    {
        formCollection.TryGetValue(key, out var value);
        var hasValue = !StringValues.IsNullOrEmpty(value);
        return !hasValue ? [] : value.AsReadOnly()!;
    }
}