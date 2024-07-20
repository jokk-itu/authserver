using System.Text;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class TokenRequestAccessorTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
	[Theory]
	[InlineData("", "")]
	[InlineData(null, "")]
	public async Task GetRequest_NormalStringParameters_ExpectNoValues(string? value, string expectedValue)
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.GrantType, value },
			{ Parameter.Code, value },
			{ Parameter.CodeVerifier, value },
			{ Parameter.RedirectUri, value },
			{ Parameter.RefreshToken, value },
			{ Parameter.ClientId, value },
			{ Parameter.ClientSecret, value },
			{ Parameter.ClientAssertion, value },
			{ Parameter.ClientAssertionType, value },
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent)
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedValue, request.GrantType);
		Assert.Equal(expectedValue, request.Code);
		Assert.Equal(expectedValue, request.CodeVerifier);
		Assert.Equal(expectedValue, request.RedirectUri);
		Assert.Equal(expectedValue, request.RefreshToken);
		Assert.Empty(request.ClientAuthentications);
	}

	[Fact]
	public async Task GetRequest_NormalStringParameters_ExpectValues()
	{
		// Arrange
		var value = "random_value";
		var expectedValue = "random_value";
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.GrantType, value },
			{ Parameter.Code, value },
			{ Parameter.CodeVerifier, value },
			{ Parameter.RedirectUri, value },
			{ Parameter.RefreshToken, value },
			{ Parameter.ClientId, value },
			{ Parameter.ClientSecret, value },
			{ Parameter.ClientAssertion, value },
			{ Parameter.ClientAssertionType, value },
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent),
				Headers =
				{
					Authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{value.FormUrlEncode()}:{value.FormUrlEncode()}"))}"
				}
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedValue, request.GrantType);
		Assert.Equal(expectedValue, request.Code);
		Assert.Equal(expectedValue, request.CodeVerifier);
		Assert.Equal(expectedValue, request.RedirectUri);
		Assert.Equal(expectedValue, request.RefreshToken);
		Assert.Equal(3, request.ClientAuthentications.Count);
	}

	[Fact]
	public async Task GetRequest_SpaceDelimitedParameters_ExpectValues()
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
		const string value = "three random values";
		string[] expectedValue = ["three", "random", "values"];
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.Scope, value },
			{ Parameter.Resource, value },
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent)
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedValue, request.Scope);
		Assert.Equal(expectedValue, request.Resource);
	}

	[Theory]
	[InlineData("", 0)]
	[InlineData(null, 0)]
	public async Task GetRequest_SpaceDelimitedParameters_ExpectZeroValues(string? value, int expectedCount)
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<TokenRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.Scope, value },
			{ Parameter.Resource, value },
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent)
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedCount, request.Scope.Count);
		Assert.Equal(expectedCount, request.Resource.Count);
	}
}