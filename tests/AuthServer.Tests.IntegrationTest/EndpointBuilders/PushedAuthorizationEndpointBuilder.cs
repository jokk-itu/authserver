using AuthServer.Constants;
using AuthServer.Options;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;
using AuthServer.Helpers;
using AuthServer.TokenDecoders;
using Xunit.Abstractions;
using AuthServer.Enums;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using ProofKeyForCodeExchangeHelper = AuthServer.Tests.Core.ProofKeyForCodeExchangeHelper;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;

public class PushedAuthorizationEndpointBuilder : EndpointBuilder
{
    private TokenEndpointAuthMethod _tokenEndpointAuthMethod;
    private bool _isProtectedWithRequestParameter;
    private string? _privateJwks;

    private List<KeyValuePair<string, string>> _parameters = [];

    public PushedAuthorizationEndpointBuilder(
        HttpClient httpClient,
        DiscoveryDocument discoveryDocument,
        JwksDocument jwksDocument,
        ITestOutputHelper testOutputHelper)
        : base(httpClient, discoveryDocument, jwksDocument, testOutputHelper)
    {
    }

    public PushedAuthorizationEndpointBuilder WithTokenEndpointAuthMethod(
        TokenEndpointAuthMethod tokenEndpointAuthMethod)
    {
        _tokenEndpointAuthMethod = tokenEndpointAuthMethod;
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithState(string state)
    {
        _parameters.Add(new(Parameter.State, state));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithResponseMode(string responseMode)
    {
        _parameters.Add(new(Parameter.ResponseMode, responseMode));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithClientId(string clientId)
    {
        _parameters.Add(new(Parameter.ClientId, clientId));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithPrompt(string prompt)
    {
        _parameters.Add(new(Parameter.Prompt, prompt));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithScope(IReadOnlyCollection<string> scope)
    {
        _parameters.Add(new(Parameter.Scope, string.Join(' ', scope)));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithCodeChallenge(string codeChallenge)
    {
        _parameters.Add(new(Parameter.CodeChallenge, codeChallenge));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithResponseType(string responseType)
    {
        _parameters.Add(new(Parameter.ResponseType, responseType));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithCodeChallengeMethod(string codeChallengeMethod)
    {
        _parameters.Add(new(Parameter.CodeChallengeMethod, codeChallengeMethod));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithNonce(string nonce)
    {
        _parameters.Add(new(Parameter.Nonce, nonce));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithMaxAge(int maxAge)
    {
        _parameters.Add(new(Parameter.MaxAge, maxAge.ToString()));
        return this;
    }

    public PushedAuthorizationEndpointBuilder WithRequest(string privateJwks)
    {
        _isProtectedWithRequestParameter = true;
        _privateJwks = privateJwks;
        return this;
    }

    internal async Task<PostPushedAuthorizationResponse> Post()
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "connect/par");

        SetDefaultValues();

        if (_tokenEndpointAuthMethod == TokenEndpointAuthMethod.ClientSecretBasic)
        {
            var clientId = _parameters.Single(x => x.Key == Parameter.ClientId).Value;
            var clientSecret = _parameters.Single(x => x.Key == Parameter.ClientSecret).Value;

            OverwriteForRequestObject();

            _parameters.RemoveAll(x => x.Key is Parameter.ClientId or Parameter.ClientSecret);

            var encodedClientId = HttpUtility.UrlEncode(clientId);
            var encodedClientSecret = HttpUtility.UrlEncode(clientSecret);
            var headerValue = $"{encodedClientId}:{encodedClientSecret}";
            var convertedHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerValue));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", convertedHeaderValue);
        }
        else
        {
            OverwriteForRequestObject();
        }

        httpRequestMessage.Content = new FormUrlEncodedContent(_parameters);
        var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage);

        TestOutputHelper.WriteLine(
            "Received Token response {0}, Content: {1}",
            httpResponseMessage.StatusCode,
            await httpResponseMessage.Content.ReadAsStringAsync());

        httpResponseMessage.EnsureSuccessStatusCode();
        return (await httpResponseMessage.Content.ReadFromJsonAsync<PostPushedAuthorizationResponse>())!;
    }

    private void SetDefaultValues()
    {
        if (_parameters.All(x => x.Key != Parameter.CodeChallenge))
        {
            _parameters.Add(new(Parameter.CodeChallenge, ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange().CodeChallenge));
        }

        if (_parameters.All(x => x.Key != Parameter.CodeChallengeMethod))
        {
            _parameters.Add(new(Parameter.CodeChallengeMethod, CodeChallengeMethodConstants.S256));
        }

        if (_parameters.All(x => x.Key != Parameter.State))
        {
            _parameters.Add(new(Parameter.State, CryptographyHelper.GetRandomString(16)));
        }

        if (_parameters.All(x => x.Key != Parameter.Nonce))
        {
            _parameters.Add(new(Parameter.Nonce, CryptographyHelper.GetRandomString(16)));
        }

        if (_parameters.All(x => x.Key != Parameter.ResponseType))
        {
            _parameters.Add(new(Parameter.ResponseType, ResponseTypeConstants.Code));
        }

        if (_parameters.All(x => x.Key != Parameter.Scope))
        {
            _parameters.Add(new(Parameter.Scope, ScopeConstants.OpenId));
        }
    }

    private void OverwriteForRequestObject()
    {
        if (!_isProtectedWithRequestParameter)
        {
            return;
        }

        var clientId = _parameters.Single(x => x.Key == Parameter.ClientId).Value;
        var claims = _parameters
            .Select(x => new KeyValuePair<string, object>(x.Key, x.Value))
            .ToDictionary();

        var requestObject = JwtBuilder.GetRequestObjectJwt(claims, clientId, _privateJwks!, ClientTokenAudience.PushedAuthorizeEndpoint);

        _parameters.Clear();
        _parameters.Add(new(Parameter.Request, requestObject));
        _parameters.Add(new(Parameter.ClientId, clientId));
    }
}