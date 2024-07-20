using System.Text;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Introspection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class IntrospectionRequesterAccessorTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
	[Theory]
	[InlineData("", "")]
	[InlineData(null, "")]
	public async Task GetRequest_NormalStringParameters_ExpectNoValues(string? value, string expectedValue)
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<IntrospectionRequest>>();
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

	[Theory]
	[InlineData("random_value", "random_value")]
	public async Task GetRequest_NormalStringParameters_ExpectValues(string? value, string expectedValue)
	{
		// Arrange
		var serviceProvider = BuildServiceProvider();
		var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<IntrospectionRequest>>();
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
				Headers =
				{
					Authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{value.FormUrlEncode()}:{value.FormUrlEncode()}"))}"
				}
			}
		};

		// Act
		var request = await requestAccessor.GetRequest(httpContext.Request);

		// Assert
		Assert.Equal(expectedValue, request.Token);
		Assert.Equal(expectedValue, request.TokenTypeHint);
		Assert.Equal(3, request.ClientAuthentications.Count);
	}
}