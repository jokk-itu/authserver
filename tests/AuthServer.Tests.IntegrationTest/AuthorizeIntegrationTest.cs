using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using AuthServer.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;
using AuthServer.Helpers;
using AuthServer.Tests.Core;
using AuthServer.TokenDecoders;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;
public class AuthorizeIntegrationTest : BaseIntegrationTest
{
    public AuthorizeIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Authorize_NoPromptWithRequestObject_ExpectRedirectCode()
    {
        // Arrange
        var httpClient = GetHttpClient();
        var jwks = ClientJwkBuilder.GetClientJwks();
        var registerResponse = await GetRegisterResponse(httpClient, jwks.PublicJwks);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        await authorizeService.CreateOrUpdateConsentGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [ScopeConstants.OpenId],
            [],
            CancellationToken.None);

        // Act
        var query = GetQuery(registerResponse.ClientId, null, ScopeConstants.OpenId, true, jwks.PrivateJwks);
        var authorizeResponseMessage = await Authorize(httpClient, query, true);
        var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponseMessage.StatusCode);
        Assert.Equal(registerResponse.RedirectUris!.Single(), authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path));
        Assert.Contains(Parameter.Code, queryNameValues.AllKeys);
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectRedirectCode()
    {
        // Arrange
        var httpClient = GetHttpClient();
        var registerResponse = await GetRegisterResponse(httpClient, null);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        await authorizeService.CreateOrUpdateConsentGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [ScopeConstants.OpenId],
            [],
            CancellationToken.None);

        // Act
        var query = GetQuery(registerResponse.ClientId, null, ScopeConstants.OpenId, false, null);
        var authorizeResponseMessage = await Authorize(httpClient, query, true);
        var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponseMessage.StatusCode);
        Assert.Equal(registerResponse.RedirectUris!.Single(), authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path));
        Assert.Contains(Parameter.Code, queryNameValues.AllKeys);
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectRedirectLogin()
    {
        // Arrange
        var httpClient = GetHttpClient();
        var registerResponse = await GetRegisterResponse(httpClient, null);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var query = GetQuery(registerResponse.ClientId, 0, ScopeConstants.OpenId, false, null);
        var authorizeResponseMessage = await Authorize(httpClient, query, true);
        var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);
        var returnUrl = queryNameValues.Get("returnUrl");

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponseMessage.StatusCode);
        Assert.Equal(UserInteraction.LoginUri, authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path));
        Assert.Equal(authorizeResponseMessage.RequestMessage!.RequestUri!.AbsoluteUri, returnUrl);
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectRedirectConsent()
    {
        // Arrange
        var httpClient = GetHttpClient();
        var registerResponse = await GetRegisterResponse(httpClient, null);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var query = GetQuery(registerResponse.ClientId, null, ScopeConstants.OpenId, false, null);
        var authorizeResponseMessage = await Authorize(httpClient, query, true);
        var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);
        var returnUrl = queryNameValues.Get("returnUrl");

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponseMessage.StatusCode);
        Assert.Equal(UserInteraction.ConsentUri, authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path));
        Assert.Equal(authorizeResponseMessage.RequestMessage!.RequestUri!.AbsoluteUri, returnUrl);
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectRedirectSelectAccount()
    {
        // Arrange
        var httpClient = GetHttpClient();
        var registerResponse = await GetRegisterResponse(httpClient, null);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var query = GetQuery(registerResponse.ClientId, null, ScopeConstants.OpenId, false, null);
        var authorizeResponseMessage = await Authorize(httpClient, query, false);
        var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);
        var returnUrl = queryNameValues.Get("returnUrl");

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponseMessage.StatusCode);
        Assert.Equal(UserInteraction.AccountSelectionUri, authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path));
        Assert.Equal(authorizeResponseMessage.RequestMessage!.RequestUri!.AbsoluteUri, returnUrl);
    }

    [Fact]
    public async Task Authorize_InvalidClientId_ExpectBadRequest()
    {
        // Arrange
        var httpClient = GetHttpClient();

        // Act
        var query = GetQuery("invalid_client_id", null, ScopeConstants.OpenId, false, null);
        var authorizeResponseMessage = await Authorize(httpClient, query, true);
        var content = await authorizeResponseMessage.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<OAuthError>(content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, authorizeResponseMessage.StatusCode);
        Assert.NotNull(error);
        Assert.Equal(ErrorCode.InvalidClient, error.Error);
        Assert.NotNull(error.ErrorDescription);
    }

    [Fact]
    public async Task Authorize_InvalidScope_ExpectRedirectInvalidScope()
    {
        // Arrange
        var httpClient = GetHttpClient();
        var registerResponse = await GetRegisterResponse(httpClient, null);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var query = GetQuery(registerResponse.ClientId, null, "invalid_scope", false, null);
        var authorizeResponseMessage = await Authorize(httpClient, query, true);
        var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);
        var error = queryNameValues.Get(Parameter.Error);
        var errorDescription = queryNameValues.Get(Parameter.ErrorDescription);

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponseMessage.StatusCode);
        Assert.Equal(registerResponse.RedirectUris!.Single(), authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path));
        Assert.Equal(ErrorCode.InvalidScope, error);
        Assert.NotNull(errorDescription);
    }

    private async Task<RegisterResponse> GetRegisterResponse(HttpClient httpClient, string? publicJwks)
    {
        var content = new Dictionary<string, object>
        {
            { Parameter.ClientName, "webapp" },
            { Parameter.RedirectUris, new[] { "https://webapp.authserver.dk/" } },
        };

        if (publicJwks is not null)
        {
            content.Add(Parameter.Jwks, publicJwks);
            content.Add(Parameter.RequestObjectSigningAlg, JwsAlgConstants.RsaSha256);
        }

        var registerResponseMessage = await httpClient.PostAsJsonAsync(
            "connect/register",
            content);
        var registerResponse = (await registerResponseMessage.Content.ReadFromJsonAsync<RegisterResponse>())!;

        TestOutputHelper.WriteLine(
            "Received Register response {0}, Content: {1}",
            registerResponseMessage.StatusCode,
            await registerResponseMessage.Content.ReadAsStringAsync());

        return registerResponse;
    }

    private async Task<HttpResponseMessage> Authorize(HttpClient httpClient, QueryString query, bool appendAuthorizeUser)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"connect/authorize{query}");

        var dataProtectorProvider = ServiceProvider.GetRequiredService<IDataProtectionProvider>();
        var dataProtector = dataProtectorProvider.CreateProtector(AuthorizeUserAccessor.DataProtectorPurpose);
        var authorizeUser = new AuthorizeUser(UserConstants.SubjectIdentifier);
        var authorizeUserBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authorizeUser));
        var encryptedAuthorizeUser = dataProtector.Protect(authorizeUserBytes);
        var cookieValue = Convert.ToBase64String(encryptedAuthorizeUser);

        if (appendAuthorizeUser)
        {
            requestMessage.Headers.Add("Cookie", new CookieHeaderValue("AuthorizeUser", cookieValue).ToString());
        }

        var authorizeResponseMessage = await httpClient.SendAsync(requestMessage);

        TestOutputHelper.WriteLine("Received Authorize response {0}, Location: {1}, Content: {2}",
            authorizeResponseMessage.StatusCode,
            authorizeResponseMessage.Headers.Location,
            await authorizeResponseMessage.Content.ReadAsStringAsync());

        return authorizeResponseMessage;
    }

    private QueryString GetQuery(string clientId, int? maxAge, string scope, bool isRequestObject, string? privateKeyJwks)
    {
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var queryBuilder = new QueryBuilder();
        if (isRequestObject)
        {
            var claims = new Dictionary<string, object>
            {
                { Parameter.ClientId, clientId },
                { Parameter.ResponseType, ResponseTypeConstants.Code },
                { Parameter.CodeChallengeMethod, CodeChallengeMethodConstants.S256 },
                { Parameter.CodeChallenge, proofKeyForCodeExchange.CodeChallenge },
                { Parameter.Scope, scope },
                { Parameter.State, CryptographyHelper.GetRandomString(32) },
                { Parameter.Nonce, CryptographyHelper.GetRandomString(32) },
            };

            if (maxAge is not null)
            {
                claims.Add(Parameter.MaxAge, maxAge);
            }

            var requestObject = JwtBuilder.GetRequestObjectJwt(claims, clientId, privateKeyJwks!, ClientTokenAudience.AuthorizeEndpoint);
            queryBuilder.Add(Parameter.Request, requestObject);
            queryBuilder.Add(Parameter.ClientId, clientId);
        }
        else
        {
            queryBuilder.Add(Parameter.ClientId, clientId);
            queryBuilder.Add(Parameter.ResponseType, ResponseTypeConstants.Code);
            queryBuilder.Add(Parameter.CodeChallengeMethod, CodeChallengeMethodConstants.S256);
            queryBuilder.Add(Parameter.CodeChallenge, proofKeyForCodeExchange.CodeChallenge);
            queryBuilder.Add(Parameter.Scope, scope);
            queryBuilder.Add(Parameter.State, CryptographyHelper.GetRandomString(32));
            queryBuilder.Add(Parameter.Nonce, CryptographyHelper.GetRandomString(32));

            if (maxAge is not null)
            {
                queryBuilder.Add(Parameter.MaxAge, maxAge.ToString()!);
            }
        }

        return queryBuilder.ToQueryString();
    }
}
