using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers;
using System.Net;
using System.Web;
using Domain.Constants;
using Infrastructure.Helpers;
using WebApp.Constants;
using Xunit;

namespace Specs.Controllers;
public class AuthorizeControllerTests : BaseIntegrationTest
{
  public AuthorizeControllerTests(WebApplicationFactory<Program> applicationFactory)
	: base(applicationFactory)
	{
  }

	[Fact]
	[Trait("Category", "Integration")]
	public async Task Authorize_ExpectRedirectResult()
	{
		// Arrange
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildClientAsync(ApplicationTypeConstants.Web, "test");
    var state = CryptographyHelper.GetRandomString(16);
    var nonce = CryptographyHelper.GetRandomString(32);
		var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var query = new QueryBuilder
    {
      { ParameterNames.ResponseType, ResponseTypeConstants.Code },
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, $"{ScopeConstants.OpenId} identityprovider:read {ScopeConstants.Profile}" },
      { ParameterNames.State, state },
      { ParameterNames.CodeChallenge, pkce.CodeChallenge },
      { ParameterNames.CodeChallengeMethod, CodeChallengeMethodConstants.S256 },
      { ParameterNames.Nonce, nonce }
    }.ToQueryString();

    // Act
    var authorizeResponse = await AuthorizeEndpointHelper.GetAuthorizationCodeAsync(Client, query, user.UserName, password);
    var queryParameters = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location!.Query);

		// Assert
		Assert.Equal(HttpStatusCode.Found, authorizeResponse.StatusCode);
		Assert.NotEmpty(queryParameters.Get("code"));
		Assert.Equal(state, queryParameters.Get("state"));
	}
}