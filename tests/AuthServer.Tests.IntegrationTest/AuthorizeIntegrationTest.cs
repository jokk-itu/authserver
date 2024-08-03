using System.Net;
using System.Net.Http.Json;
using System.Web;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;
using AuthServer.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;
public class AuthorizeIntegrationTest : BaseIntegrationTest
{
    public AuthorizeIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Authorize_NoPrompt_ExpectLoginRequired()
    {
        // Arrange
        var httpClient = GetHttpClient();
        var registerResponseMessage = await httpClient.PostAsJsonAsync(
            "connect/register",
            new Dictionary<string, object>
            {
                { Parameter.ClientName, "webapp" },
                { Parameter.RedirectUris, new[] { "https://webapp.authserver.dk" } },
            });
        var registerResponse = (await registerResponseMessage.Content.ReadFromJsonAsync<RegisterResponse>())!;

        var proofKeyForCodeExchange = ProofKeyForCodeExchangeHelper.GetProofKeyForCodeExchange();
        var query = new QueryBuilder
        {
            { Parameter.ClientId, registerResponse.ClientId },
            { Parameter.ResponseMode, ResponseModeConstants.Query },
            { Parameter.ResponseType, ResponseTypeConstants.Code },
            { Parameter.CodeChallengeMethod, CodeChallengeMethodConstants.S256 },
            { Parameter.CodeChallenge, proofKeyForCodeExchange.CodeChallenge },
            { Parameter.Scope, ScopeConstants.OpenId },
            { Parameter.State, CryptographyHelper.GetRandomString(32) },
            { Parameter.Nonce, CryptographyHelper.GetRandomString(32) }
        }.ToQueryString();

        // Act
        var authorizeResponseMessage = await httpClient.GetAsync($"connect/authorize{query}");
        var queryNameValues = HttpUtility.ParseQueryString(authorizeResponseMessage.Headers.Location!.Query);
        var returnUrl = queryNameValues.Get("returnUrl");

        // Assert
        Assert.Equal(HttpStatusCode.SeeOther, authorizeResponseMessage.StatusCode);
        Assert.Equal(UserInteraction.LoginUri, authorizeResponseMessage.Headers.Location!.GetLeftPart(UriPartial.Path));
        Assert.Equal(authorizeResponseMessage.RequestMessage!.RequestUri!.AbsoluteUri, returnUrl);
    }
}
