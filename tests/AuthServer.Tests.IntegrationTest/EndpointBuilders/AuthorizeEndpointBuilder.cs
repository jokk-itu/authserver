using System.Net;
using AuthServer.Authorize;
using AuthServer.Core;
using AuthServer.Options;
using AuthServer.Tests.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AuthServer.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Xunit.Abstractions;
using AuthServer.Constants;
using System.Web;
using System.Text.RegularExpressions;
using AuthServer.TokenDecoders;
using ProofKeyForCodeExchangeHelper = AuthServer.Tests.Core.ProofKeyForCodeExchangeHelper;
using AuthServer.Endpoints.Responses;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;
public class AuthorizeEndpointBuilder : EndpointBuilder
{
    private readonly IDataProtectionProvider _dataProtectionProvider;

    private bool _isProtectedWithRequestParameter;
    private string? _privateJwks;
    private string _clientId;

    private List<KeyValuePair<string, object>> _parameters = [];
    private readonly List<CookieHeaderValue> _cookies = [];

    public AuthorizeEndpointBuilder(
        HttpClient httpClient,
        IDataProtectionProvider dataProtectionProvider,
        DiscoveryDocument discoveryDocument,
        JwksDocument jwksDocument,
        ITestOutputHelper testOutputHelper)
        : base(httpClient, discoveryDocument, jwksDocument, testOutputHelper)
    {
        _dataProtectionProvider = dataProtectionProvider;
    }

    public AuthorizeEndpointBuilder WithState(string state)
    {
        _parameters.Add(new(Parameter.State, state));
        return this;
    }

    public AuthorizeEndpointBuilder WithResponseMode(string responseMode)
    {
        _parameters.Add(new(Parameter.ResponseMode, responseMode));
        return this;
    }

    public AuthorizeEndpointBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        _parameters.Add(new(Parameter.ClientId, clientId));
        return this;
    }

    public AuthorizeEndpointBuilder WithPrompt(string prompt)
    {
        _parameters.Add(new(Parameter.Prompt, prompt));
        return this;
    }

    public AuthorizeEndpointBuilder WithScope(IReadOnlyCollection<string> scope)
    {
        _parameters.Add(new(Parameter.Scope, string.Join(' ', scope)));
        return this;
    }

    public AuthorizeEndpointBuilder WithCodeChallenge(string codeChallenge)
    {
        _parameters.Add(new(Parameter.CodeChallenge, codeChallenge));
        return this;
    }

    public AuthorizeEndpointBuilder WithResponseType(string responseType)
    {
        _parameters.Add(new(Parameter.ResponseType, responseType));
        return this;
    }

    public AuthorizeEndpointBuilder WithCodeChallengeMethod(string codeChallengeMethod)
    {
        _parameters.Add(new(Parameter.CodeChallengeMethod, codeChallengeMethod));
        return this;
    }

    public AuthorizeEndpointBuilder WithNonce(string nonce)
    {
        _parameters.Add(new(Parameter.Nonce, nonce));
        return this;
    }

    public AuthorizeEndpointBuilder WithMaxAge(int maxAge)
    {
        _parameters.Add(new(Parameter.MaxAge, maxAge.ToString()));
        return this;
    }

    public AuthorizeEndpointBuilder WithAuthorizeUser()
    {
        var dataProtector = _dataProtectionProvider.CreateProtector(AuthorizeUserAccessor.DataProtectorPurpose);
        var authorizeUser = new AuthorizeUser(UserConstants.SubjectIdentifier);
        var authorizeUserBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authorizeUser));
        var encryptedAuthorizeUser = dataProtector.Protect(authorizeUserBytes);
        var cookieValue = Convert.ToBase64String(encryptedAuthorizeUser);
        _cookies.Add(new CookieHeaderValue(AuthorizeUserAccessor.AuthorizeUserCookieName, cookieValue));
        return this;
    }

    public AuthorizeEndpointBuilder WithRequest(string privateJwks)
    {
        _isProtectedWithRequestParameter = true;
        _privateJwks = privateJwks;
        return this;
    }

    public async Task<AuthorizeResponse> Get()
    {
        SetDefaultValues();
        OverwriteForRequestObject();

        var query = new QueryBuilder(_parameters.ToDictionary(x => x.Key, x => x.Value.ToString()!)).ToQueryString();
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"connect/authorize{query}");
        requestMessage.Headers.Add("Cookie", _cookies.Select(x => x.ToString()));
        var authorizeResponseMessage = await HttpClient.SendAsync(requestMessage);

        TestOutputHelper.WriteLine("Received Authorize response {0}, Location: {1}, Content: {2}",
            authorizeResponseMessage.StatusCode,
            authorizeResponseMessage.Headers.Location,
            await authorizeResponseMessage.Content.ReadAsStringAsync());

        return await GetAuthorizeResponse(authorizeResponseMessage);
    }

    private async Task<AuthorizeResponse> GetAuthorizeResponse(HttpResponseMessage authorizeResponseMessage)
    {
        if (authorizeResponseMessage.StatusCode == HttpStatusCode.SeeOther)
        {
            var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);
            return new AuthorizeResponse
            {
                StatusCode = HttpStatusCode.SeeOther,
                Code = queryNameValues.Get(Parameter.Code),
                State = queryNameValues.Get(Parameter.State),
                Error = queryNameValues.Get(Parameter.Error),
                ErrorDescription = queryNameValues.Get(Parameter.ErrorDescription),
                ReturnUrl = queryNameValues.Get("returnUrl"),
                LocationUri = authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path),
                RequestUri = authorizeResponseMessage.RequestMessage!.RequestUri!.AbsoluteUri!
            };
        }

        if (authorizeResponseMessage.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await authorizeResponseMessage.Content.ReadAsStringAsync();
            var oAuthError = JsonSerializer.Deserialize<OAuthError>(content)!;
            return new AuthorizeResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Error = oAuthError.Error,
                ErrorDescription = oAuthError?.ErrorDescription
            };
        }

        if (authorizeResponseMessage.StatusCode == HttpStatusCode.OK)
        {
            var content = await authorizeResponseMessage.Content.ReadAsStringAsync();

            var codeInput = Regex.Match(content, @"<input type=""hidden"" name=""code"" value=""([^""]+)"" \/>");
            var code = codeInput.Groups[1].Captures[0].Value;

            var stateInput = Regex.Match(content, @"<input type=""hidden"" name=""state"" value=""([^""]+)"" \/>");
            var state = stateInput.Groups[1].Captures[0].Value;

            var errorInput = Regex.Match(content, @"<input type=""hidden"" name=""error"" value=""([^""]+)"" \/>");
            var error = errorInput.Groups[1].Captures[0].Value;

            var errorDescriptionInput = Regex.Match(content, @"<input type=""hidden"" name=""error_description"" value=""([^""]+)"" \/>");
            var errorDescription = errorDescriptionInput.Groups[1].Captures[0].Value;

            return new AuthorizeResponse
            {
                StatusCode = HttpStatusCode.OK,
                Code = code,
                State = state,
                Error = error,
                ErrorDescription = errorDescription
            };
        }

        throw new NotSupportedException();
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

        var requestObject = JwtBuilder.GetRequestObjectJwt(_parameters.ToDictionary(), _clientId, _privateJwks!, ClientTokenAudience.AuthorizeEndpoint);
        _parameters.Clear();
        _parameters.Add(new(Parameter.Request, requestObject));
        _parameters.Add(new(Parameter.ClientId, _clientId));
    }

    public sealed record AuthorizeResponse
    {
        public HttpStatusCode StatusCode { get; init; }
        public string? Code { get; init; }
        public string? Issuer { get; init; }
        public string? State { get; init; }
        public string? Error { get; init; }
        public string? ErrorDescription { get; init; }

        // Query parameter from local redirects
        public string? ReturnUrl { get; init; }

        // Uri from the Location header
        public string? LocationUri { get; init; }

        // 
        public string? RequestUri { get; init; }
    }
}