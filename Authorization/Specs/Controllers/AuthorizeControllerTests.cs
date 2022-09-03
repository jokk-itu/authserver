using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Xunit;

namespace Specs.Controllers;
public class AuthorizeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _applicationFactory;

	public AuthorizeControllerTests(WebApplicationFactory<Program> applicationFactory)
	{
		_applicationFactory = applicationFactory;
	}

	[Fact]
	[Trait("Category", "Integration")]
	public async Task Authorize_ExpectRedirectResult()
	{
		// Arrange
		var client = _applicationFactory.CreateClient(new WebApplicationFactoryClientOptions 
		{
			 AllowAutoRedirect = false
		});

    var state = "rsbghiwsbgiwebgiuwbgiwubgebggbweige";
    var nonce = "wrjibgnwiubgwieubgiwuqebgiwbgiuwebgfiuwebgiuwebgiuwebgweiu";
    var codeChallenge = "ijwbnfiwebgfuibewgjbwseijgbweiugbieuwwkbfgwiuebgiwuebgibweiugweibgieb";
    var query = new QueryBuilder
    {
      { "response_type", "code" },
      { "client_id", "test" },
      { "redirect_uri", "http://localhost:5002/callback" },
      { "scope", "openid identity-provider profile api1" },
      { "state", state },
      { "code_challenge", codeChallenge },
      { "code_challenge_method", "S256" },
      { "nonce", nonce }
    }.ToQueryString();

    // Act
    var loginViewResponse = await client.GetAsync($"connect/v1/authorize{query}");
		var loginViewHtml = await loginViewResponse.Content.ReadAsStringAsync();

    var antiForgeryCookie = loginViewResponse.Headers
			.GetValues("Set-Cookie")
			.FirstOrDefault(x => x.Contains("AntiForgeryCookie"));

		var antiForgeryCookieValue = SetCookieHeaderValue.Parse(antiForgeryCookie).Value;
		var antiForgeryFieldMatch = Regex.Match(loginViewHtml, $@"\<input name=""AntiForgeryField"" type=""hidden"" value=""([^""]+)"" \/\>");
		var antiForgeryField = antiForgeryFieldMatch.Groups[1].Captures[0].Value;

		var postAuthorizeRequest = new HttpRequestMessage(HttpMethod.Post, $"connect/v1/authorize{query}");
		postAuthorizeRequest.Headers.Add("Cookie", new CookieHeaderValue("AntiForgeryCookie", antiForgeryCookieValue).ToString());
		var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{ "username", "jokk" },
			{ "password", "Password12!" },
			{ "AntiForgeryField", antiForgeryField }
		});
		postAuthorizeRequest.Content = loginForm;
		var authorizeResponse = await client.SendAsync(postAuthorizeRequest);
		var queryParameters = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location!.Query);

		// Assert
		Assert.Equal(HttpStatusCode.Found, authorizeResponse.StatusCode);
		Assert.NotEmpty(queryParameters.Get("code"));
		Assert.Equal(state, queryParameters.Get("state"));
	}
}