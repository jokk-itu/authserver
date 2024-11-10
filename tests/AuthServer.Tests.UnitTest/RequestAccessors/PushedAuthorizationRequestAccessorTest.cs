using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.PushedAuthorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Text;
using AuthServer.Authentication.Models;
using AuthServer.Enums;
using AuthServer.TokenDecoders;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class PushedAuthorizationRequestAccessorTest : BaseUnitTest
{
    public PushedAuthorizationRequestAccessorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    public async Task GetRequest_EmptyStringParametersBody_ExpectValues(string? value, string expectedValue)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<PushedAuthorizationRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                Form = new FormCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.LoginHint, value },
                    { Parameter.Display, value },
                    { Parameter.ResponseMode, value },
                    { Parameter.MaxAge, value },
                    { Parameter.CodeChallenge, value },
                    { Parameter.CodeChallengeMethod, value },
                    { Parameter.RedirectUri, value },
                    { Parameter.IdTokenHint, value },
                    { Parameter.Prompt, value },
                    { Parameter.ResponseType, value },
                    { Parameter.Nonce, value },
                    { Parameter.State, value },
                    { Parameter.Request, value },
                    { Parameter.ClientId, value },
                    { Parameter.ClientSecret, value },
                    { Parameter.ClientAssertion, value },
                    { Parameter.ClientAssertionType, value },
                }),
                Headers =
                {
                    Authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{value.FormUrlEncode()}:{value.FormUrlEncode()}"))}"
                }
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.LoginHint);
        Assert.Equal(expectedValue, request.Display);
        Assert.Equal(expectedValue, request.ResponseMode);
        Assert.Equal(expectedValue, request.MaxAge);
        Assert.Equal(expectedValue, request.CodeChallenge);
        Assert.Equal(expectedValue, request.CodeChallengeMethod);
        Assert.Equal(expectedValue, request.RedirectUri);
        Assert.Equal(expectedValue, request.IdTokenHint);
        Assert.Equal(expectedValue, request.Prompt);
        Assert.Equal(expectedValue, request.ResponseType);
        Assert.Equal(expectedValue, request.Nonce);
        Assert.Equal(expectedValue, request.State);
        Assert.Equal(expectedValue, request.RequestObject);
        Assert.Empty(request.ClientAuthentications);
    }

    [Fact]
    public async Task GetRequest_NormalStringParameters_ExpectValues()
    {
        // Arrange
        const string value = "random_value";
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<PushedAuthorizationRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                Form = new FormCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.LoginHint, value },
                    { Parameter.Display, value },
                    { Parameter.ResponseMode, value },
                    { Parameter.MaxAge, value },
                    { Parameter.CodeChallenge, value },
                    { Parameter.CodeChallengeMethod, value },
                    { Parameter.RedirectUri, value },
                    { Parameter.IdTokenHint, value },
                    { Parameter.Prompt, value },
                    { Parameter.ResponseType, value },
                    { Parameter.Nonce, value },
                    { Parameter.State, value },
                    { Parameter.Request, value },
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(value, request.LoginHint);
        Assert.Equal(value, request.Display);
        Assert.Equal(value, request.ResponseMode);
        Assert.Equal(value, request.MaxAge);
        Assert.Equal(value, request.CodeChallenge);
        Assert.Equal(value, request.CodeChallengeMethod);
        Assert.Equal(value, request.RedirectUri);
        Assert.Equal(value, request.IdTokenHint);
        Assert.Equal(value, request.Prompt);
        Assert.Equal(value, request.ResponseType);
        Assert.Equal(value, request.Nonce);
        Assert.Equal(value, request.State);
        Assert.Equal(value, request.RequestObject);
        Assert.Empty(request.ClientAuthentications);
    }

    [Fact]
    public async Task GetRequest_NormalStringParametersForClientAuthentication_ExpectValues()
    {
        // Arrange
        const string value = "random_value";
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<PushedAuthorizationRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                Form = new FormCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.ClientId, value },
                    { Parameter.ClientSecret, value },
                    { Parameter.ClientAssertion, value },
                    { Parameter.ClientAssertionType, value },
                }),
                Headers =
                {
                    Authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{value.FormUrlEncode()}:{value.FormUrlEncode()}"))}"
                }
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Collection(request.ClientAuthentications,
            clientAuthentication =>
            {
                Assert.IsType<ClientSecretAuthentication>(clientAuthentication);
                var clientSecretAuthentication = (clientAuthentication as ClientSecretAuthentication)!;
                Assert.Equal(value, clientSecretAuthentication.ClientId);
                Assert.Equal(value, clientSecretAuthentication.ClientSecret);
                Assert.Equal(TokenEndpointAuthMethod.ClientSecretBasic, clientSecretAuthentication.Method);
            },
            clientAuthentication =>
            {
                Assert.IsType<ClientSecretAuthentication>(clientAuthentication);
                var clientSecretAuthentication = (clientAuthentication as ClientSecretAuthentication)!;
                Assert.Equal(value, clientSecretAuthentication.ClientId);
                Assert.Equal(value, clientSecretAuthentication.ClientSecret);
                Assert.Equal(TokenEndpointAuthMethod.ClientSecretPost, clientSecretAuthentication.Method);
            },
            clientAuthentication =>
            {
                Assert.IsType<ClientAssertionAuthentication>(clientAuthentication);
                var clientAssertionAuthentication = (clientAuthentication as ClientAssertionAuthentication)!;
                Assert.Equal(value, clientAssertionAuthentication.ClientId);
                Assert.Equal(value, clientAssertionAuthentication.ClientAssertion);
                Assert.Equal(value, clientAssertionAuthentication.ClientAssertionType);
                Assert.Equal(ClientTokenAudience.PushedAuthorizeEndpoint, clientAssertionAuthentication.Audience);
                Assert.Equal(TokenEndpointAuthMethod.PrivateKeyJwt, clientAssertionAuthentication.Method);
            });
    }

    [Fact]
    public async Task GetRequest_SpaceDelimitedParametersBody_ExpectValues()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<PushedAuthorizationRequest>>();
        const string value = "three random values";
        string[] expectedValue = ["three", "random", "values"];
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                Form = new FormCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.Scope, value },
                    { Parameter.AcrValues, value },
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedValue, request.Scope);
        Assert.Equal(expectedValue, request.AcrValues);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public async Task GetRequest_SpaceDelimitedParametersBody_ExpectZeroValues(string? value, int expectedCount)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<PushedAuthorizationRequest>>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                Form = new FormCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.Scope, value },
                    { Parameter.AcrValues, value },
                })
            }
        };

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(expectedCount, request.Scope.Count);
        Assert.Equal(expectedCount, request.AcrValues.Count);
    }
}
