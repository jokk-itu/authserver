using AuthServer.Constants;
using AuthServer.Enums;
using AuthServer.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using AuthServer.Core;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;

public class RevocationEndpointBuilder : EndpointBuilder
{
    private TokenEndpointAuthMethod _tokenEndpointAuthMethod;
    private readonly List<KeyValuePair<string, string>> _parameters = [];

    public RevocationEndpointBuilder(
        HttpClient httpClient,
        DiscoveryDocument discoveryDocument,
        JwksDocument jwksDocument,
        ITestOutputHelper testOutputHelper)
        : base(httpClient, discoveryDocument, jwksDocument, testOutputHelper)
    {
    }

    public RevocationEndpointBuilder WithTokenTypeHint(string tokenTypeHint)
    {
        _parameters.Add(new(Parameter.TokenTypeHint, tokenTypeHint));
        return this;
    }

    public RevocationEndpointBuilder WithToken(string token)
    {
        _parameters.Add(new(Parameter.Token, token));
        return this;
    }

    public RevocationEndpointBuilder WithClientId(string clientId)
    {
        _parameters.Add(new(Parameter.ClientId, clientId));
        return this;
    }

    public RevocationEndpointBuilder WithClientSecret(string clientSecret)
    {
        _parameters.Add(new(Parameter.ClientSecret, clientSecret));
        return this;
    }

    public RevocationEndpointBuilder WithClientAssertion(string clientAssertion)
    {
        _parameters.Add(new(Parameter.ClientAssertion, clientAssertion));
        _parameters.Add(new(Parameter.ClientAssertionType, ClientAssertionTypeConstants.PrivateKeyJwt));
        return this;
    }

    public RevocationEndpointBuilder WithTokenEndpointAuthMethod(TokenEndpointAuthMethod tokenEndpointAuthMethod)
    {
        _tokenEndpointAuthMethod = tokenEndpointAuthMethod;
        return this;
    }

    internal async Task Post()
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "connect/revoke");

        if (_tokenEndpointAuthMethod == TokenEndpointAuthMethod.ClientSecretBasic)
        {
            var clientId = _parameters.Single(x => x.Key == Parameter.ClientId).Value;
            var clientSecret = _parameters.Single(x => x.Key == Parameter.ClientSecret).Value;
            
            _parameters.RemoveAll(x => x.Key is Parameter.ClientId or Parameter.ClientSecret);

            var encodedClientId = HttpUtility.UrlEncode(clientId);
            var encodedClientSecret = HttpUtility.UrlEncode(clientSecret);
            var headerValue = $"{encodedClientId}:{encodedClientSecret}";
            var convertedHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerValue));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", convertedHeaderValue);
        }

        httpRequestMessage.Content = new FormUrlEncodedContent(_parameters);
        var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage);

        TestOutputHelper.WriteLine(
            "Received Revocation response {0}, Content: {1}",
            httpResponseMessage.StatusCode,
            await httpResponseMessage.Content.ReadAsStringAsync());

        httpResponseMessage.EnsureSuccessStatusCode();
    }
}