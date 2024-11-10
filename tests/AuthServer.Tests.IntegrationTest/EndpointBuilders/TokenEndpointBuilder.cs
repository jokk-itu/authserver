using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;
using AuthServer.Enums;
using AuthServer.Options;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;

public class TokenEndpointBuilder : EndpointBuilder
{
    private TokenEndpointAuthMethod _tokenEndpointAuthMethod = TokenEndpointAuthMethod.ClientSecretBasic;

    private List<KeyValuePair<string, string>> _parameters = [];

    public TokenEndpointBuilder(HttpClient httpClient, DiscoveryDocument discoveryDocument, JwksDocument jwksDocument, ITestOutputHelper testOutputHelper)
        : base(httpClient, discoveryDocument, jwksDocument, testOutputHelper)
    {
    }

    public TokenEndpointBuilder WithCodeVerifier(string codeVerifier)
    {
        _parameters.Add(new(Parameter.CodeVerifier, codeVerifier));
        return this;
    }

    public TokenEndpointBuilder WithCode(string code)
    {
        _parameters.Add(new(Parameter.Code, code));
        return this;
    }

    public TokenEndpointBuilder WithRefreshToken(string refreshToken)
    {
        _parameters.Add(new(Parameter.RefreshToken, refreshToken));
        return this;
    }

    public TokenEndpointBuilder WithScope(IReadOnlyCollection<string> scope)
    {
        _parameters.Add(new(Parameter.Scope, string.Join(' ', scope)));
        return this;
    }

    public TokenEndpointBuilder WithResource(IReadOnlyCollection<string> resources)
    {
        resources.ToList().ForEach(x => _parameters.Add(new(Parameter.Resource, x)));
        return this;
    }

    public TokenEndpointBuilder WithGrantType(string grantType)
    {
        _parameters.Add(new(Parameter.GrantType, grantType));
        return this;
    }

    public TokenEndpointBuilder WithClientId(string clientId)
    {
        _parameters.Add(new(Parameter.ClientId, clientId));
        return this;
    }

    public TokenEndpointBuilder WithClientSecret(string clientSecret)
    {
        _parameters.Add(new(Parameter.ClientSecret, clientSecret));
        return this;
    }

    public TokenEndpointBuilder WithClientAssertion(string clientAssertion)
    {
        _parameters.Add(new(Parameter.ClientAssertion, clientAssertion));
        _parameters.Add(new(Parameter.ClientAssertionType, ClientAssertionTypeConstants.PrivateKeyJwt));
        return this;
    }

    public TokenEndpointBuilder WithTokenEndpointAuthMethod(TokenEndpointAuthMethod tokenEndpointAuthMethod)
    {
        _tokenEndpointAuthMethod = tokenEndpointAuthMethod;
        return this;
    }

    internal async Task<PostTokenResponse> Post()
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "connect/token");

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
            "Received Token response {0}, Content: {1}",
            httpResponseMessage.StatusCode,
            await httpResponseMessage.Content.ReadAsStringAsync());

        httpResponseMessage.EnsureSuccessStatusCode();
        return (await httpResponseMessage.Content.ReadFromJsonAsync<PostTokenResponse>())!;
    }
}
