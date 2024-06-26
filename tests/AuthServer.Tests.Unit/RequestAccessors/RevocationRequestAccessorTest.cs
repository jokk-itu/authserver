using AuthServer.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Text;
using AuthServer.RequestAccessors.Revocation;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;

namespace AuthServer.Tests.Unit.RequestAccessors;

public class RevocationRequestAccessorTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
	[Theory]
	[InlineData("", "")]
	[InlineData(null, "")]
	public async Task GetRequest_NormalStringParameters_ExpectNoValues(string? value, string expectedValue)
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RevocationRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.Token, value },
			{ Parameter.TokenTypeHint, value },
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
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedValue, request.Token);
		Assert.Equal(expectedValue, request.TokenTypeHint);
		Assert.Empty(request.ClientAuthentications);
	}

	[Fact]
	public async Task GetRequest_NormalStringParameters_ExpectValues()
	{
		// Arrange
		var actual = "random_value";
		var expected = "random_value";
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RevocationRequest>>();
		var formUrlContent = new Dictionary<string, StringValues>
		{
			{ Parameter.Token, actual },
			{ Parameter.TokenTypeHint, actual },
			{ Parameter.ClientId, actual },
			{ Parameter.ClientSecret, actual },
			{ Parameter.ClientAssertion, actual },
			{ Parameter.ClientAssertionType, actual },
		};

		var httpContext = new DefaultHttpContext
		{
			Request =
			{
				Form = new FormCollection(formUrlContent),
				Headers =
				{
					Authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{actual.FormUrlEncode()}:{actual.FormUrlEncode()}"))}"
				}
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expected, request.Token);
		Assert.Equal(expected, request.TokenTypeHint);
		Assert.Equal(3, request.ClientAuthentications.Count);
	}
}