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
using Microsoft.AspNetCore.DataProtection;
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
    public async Task Authorize_NoPrompt_ExpectRedirectCode()
    {
        // Arrange
        var httpClient = GetHttpClient();
        var registerResponse = await GetRegisterResponse(httpClient);

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
        var authorizeResponseMessage = await Authorize(httpClient, registerResponse.ClientId, null, true, ScopeConstants.OpenId);
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
        var registerResponse = await GetRegisterResponse(httpClient);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var authorizeResponseMessage = await Authorize(httpClient, registerResponse.ClientId, 0, true, ScopeConstants.OpenId);
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
        var registerResponse = await GetRegisterResponse(httpClient);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var authorizeResponseMessage = await Authorize(httpClient, registerResponse.ClientId, null, true, ScopeConstants.OpenId);
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
        var registerResponse = await GetRegisterResponse(httpClient);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var authorizeResponseMessage = await Authorize(httpClient, registerResponse.ClientId, null, false, ScopeConstants.OpenId);
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
        var authorizeResponseMessage = await Authorize(httpClient, "invalid_client_id", null, false, ScopeConstants.OpenId);
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
        var registerResponse = await GetRegisterResponse(httpClient);

        await AddUser();
        await AddAuthenticationContextReferences();

        var authorizeService = ServiceProvider.GetRequiredService<IAuthorizeService>();
        await authorizeService.CreateAuthorizationGrant(
            UserConstants.SubjectIdentifier,
            registerResponse.ClientId,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Act
        var authorizeResponseMessage = await Authorize(httpClient, registerResponse.ClientId, 0, true, "invalid_scope");
        var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);
        var error = queryNameValues.Get(Parameter.Error);
        var errorDescription = queryNameValues.Get(Parameter.ErrorDescription);

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponseMessage.StatusCode);
        Assert.Equal(registerResponse.RedirectUris!.Single(), authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path));
        Assert.Equal(ErrorCode.InvalidScope, error);
        Assert.NotNull(errorDescription);
    }

    private async Task<RegisterResponse> GetRegisterResponse(HttpClient httpClient)
    {
        var registerResponseMessage = await httpClient.PostAsJsonAsync(
            "connect/register",
            new Dictionary<string, object>
            {
                { Parameter.ClientName, "webapp" },
                { Parameter.RedirectUris, new[] { "https://webapp.authserver.dk/" } },
            });
        var registerResponse = (await registerResponseMessage.Content.ReadFromJsonAsync<RegisterResponse>())!;

        TestOutputHelper.WriteLine(
            "Received Register response {0}, Content: {1}",
            registerResponseMessage.StatusCode,
            await registerResponseMessage.Content.ReadAsStringAsync());

        return registerResponse;
    }

    private async Task<HttpResponseMessage> Authorize(HttpClient httpClient, string clientId, int? maxAge, bool appendAuthorizeUser, string scope)
    {
        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var queryBuilder = new QueryBuilder
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
            queryBuilder.Add(Parameter.MaxAge, maxAge.ToString()!);
        }

        var query = queryBuilder.ToQueryString();
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
}
