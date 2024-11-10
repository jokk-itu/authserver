using AuthServer.Constants;
using AuthServer.Endpoints.Responses;
using AuthServer.Enums;
using AuthServer.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using AuthServer.Core;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;

public class IntrospectionEndpointBuilder : EndpointBuilder
{
    private TokenEndpointAuthMethod _tokenEndpointAuthMethod;
    private readonly List<KeyValuePair<string, string>> _parameters = [];

    public IntrospectionEndpointBuilder(
        HttpClient httpClient,
        DiscoveryDocument discoveryDocument,
        JwksDocument jwksDocument,
        ITestOutputHelper testOutputHelper)
        : base(httpClient, discoveryDocument, jwksDocument, testOutputHelper)
    {
    }

    public IntrospectionEndpointBuilder WithTokenTypeHint(string tokenTypeHint)
    {
        _parameters.Add(new(Parameter.TokenTypeHint, tokenTypeHint));
        return this;
    }

    public IntrospectionEndpointBuilder WithToken(string token)
    {
        _parameters.Add(new(Parameter.Token, token));
        return this;
    }

    public IntrospectionEndpointBuilder WithClientId(string clientId)
    {
        _parameters.Add(new(Parameter.ClientId, clientId));
        return this;
    }

    public IntrospectionEndpointBuilder WithClientSecret(string clientSecret)
    {
        _parameters.Add(new(Parameter.ClientSecret, clientSecret));
        return this;
    }

    public IntrospectionEndpointBuilder WithClientAssertion(string clientAssertion)
    {
        _parameters.Add(new(Parameter.ClientAssertion, clientAssertion));
        _parameters.Add(new(Parameter.ClientAssertionType, ClientAssertionTypeConstants.PrivateKeyJwt));
        return this;
    }

    public IntrospectionEndpointBuilder WithTokenEndpointAuthMethod(TokenEndpointAuthMethod tokenEndpointAuthMethod)
    {
        _tokenEndpointAuthMethod = tokenEndpointAuthMethod;
        return this;
    }

    internal async Task<PostIntrospectionResponse> Post()
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "connect/introspection");

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
            "Received Introspection response {0}, Content: {1}",
            httpResponseMessage.StatusCode,
            await httpResponseMessage.Content.ReadAsStringAsync());

        httpResponseMessage.EnsureSuccessStatusCode();
        return (await httpResponseMessage.Content.ReadFromJsonAsync<PostIntrospectionResponse>())!;
    }
}