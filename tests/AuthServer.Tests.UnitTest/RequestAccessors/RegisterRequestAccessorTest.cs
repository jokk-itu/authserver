using System.Security.Claims;
using System.Text.Json;
using AuthServer.Authentication.OAuthToken;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.RequestAccessors.Register;
using AuthServer.Tests.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class RegisterRequestAccessorTest : BaseUnitTest
{
    public RegisterRequestAccessorTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [InlineData("POST", "random_value", "random_value")]
    [InlineData("PUT", "random_value", "random_value")]
    [InlineData("POST", "", "")]
    [InlineData("PUT", "", "")]
    [InlineData("POST", null, null)]
    [InlineData("PUT", null, null)]
    public async Task GetRequest_StringParametersPostAndPut_ExpectValues(string method, string? value, string? expectedValue)
    {
        // Arrange
        var requestContent = new Dictionary<string, string?>
        {
            { Parameter.ClientName, value },
            { Parameter.ApplicationType, value },
            { Parameter.SubjectType, value },
            { Parameter.DefaultMaxAge, value },
            { Parameter.TokenEndpointAuthMethod, value },
            { Parameter.TokenEndpointAuthSigningAlg, value },
            { Parameter.Jwks, value },

            { Parameter.JwksUri, value },
            { Parameter.ClientUri, value },
            { Parameter.PolicyUri, value },
            { Parameter.TosUri, value },
            { Parameter.InitiateLoginUri, value },
            { Parameter.LogoUri, value },
            { Parameter.BackchannelLogoutUri, value },
            { Parameter.SectorIdentifierUri, value },

            { Parameter.RequireSignedRequestObject, value },
            { Parameter.RequireReferenceToken, value },
            { Parameter.RequirePushedAuthorizationRequests, value },

            { Parameter.RequestObjectEncryptionEnc, value },
            { Parameter.RequestObjectEncryptionAlg, value },
            { Parameter.RequestObjectSigningAlg, value },

            { Parameter.UserinfoEncryptedResponseEnc, value },
            { Parameter.UserinfoEncryptedResponseAlg, value },
            { Parameter.UserinfoSignedResponseAlg, value },

            { Parameter.IdTokenEncryptedResponseEnc, value },
            { Parameter.IdTokenEncryptedResponseAlg, value },
            { Parameter.IdTokenSignedResponseAlg, value }
        };
        var requestJson = JsonSerializer.SerializeToUtf8Bytes(requestContent);
        var requestStream = new MemoryStream(requestJson);
        var queryContent = new Dictionary<string, StringValues>();
        if (method == "PUT")
        {
            queryContent.Add(Parameter.ClientId, value);
        }
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = method,
                Body = requestStream,
                ContentLength = requestStream.Length,
                ContentType = MimeTypeConstants.Json,
                Query = new QueryCollection(queryContent)
            },
        };

        var serviceProvider = BuildServiceProvider(services =>
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var authResult = AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(), OAuthTokenAuthenticationDefaults.AuthenticationScheme));

            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = Parameter.AccessToken, Value = value }
            });

            authenticationServiceMock
                .Setup(x => x.AuthenticateAsync(httpContext, OAuthTokenAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(authResult);

            services.AddScopedMock(authenticationServiceMock);
        });
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RegisterRequest>>();
        httpContext.RequestServices = serviceProvider;

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(method, request.Method.Method);
        if (method == "POST")
        {
            Assert.Null(request.ClientId);
            Assert.Null(request.RegistrationAccessToken);
        }
        else if (method == "PUT")
        {
            Assert.Equal(expectedValue, request.ClientId);
            Assert.Equal(expectedValue, request.RegistrationAccessToken);
        }
        

        Assert.Equal(expectedValue, request.ClientName);
        Assert.Equal(expectedValue, request.ApplicationType);
        Assert.Equal(expectedValue, request.SubjectType);
        Assert.Equal(expectedValue, request.DefaultMaxAge);
        Assert.Equal(expectedValue, request.TokenEndpointAuthMethod);
        Assert.Equal(expectedValue, request.Jwks);

        Assert.Equal(expectedValue, request.JwksUri);
        Assert.Equal(expectedValue, request.ClientUri);
        Assert.Equal(expectedValue, request.PolicyUri);
        Assert.Equal(expectedValue, request.TosUri);
        Assert.Equal(expectedValue, request.InitiateLoginUri);
        Assert.Equal(expectedValue, request.LogoUri);
        Assert.Equal(expectedValue, request.BackchannelLogoutUri);
        Assert.Equal(expectedValue, request.SectorIdentifierUri);

        Assert.Equal(expectedValue, request.RequestObjectEncryptionEnc);
        Assert.Equal(expectedValue, request.RequestObjectEncryptionAlg);
        Assert.Equal(expectedValue, request.RequestObjectSigningAlg);

        Assert.Equal(expectedValue, request.UserinfoEncryptedResponseEnc);
        Assert.Equal(expectedValue, request.UserinfoEncryptedResponseAlg);
        Assert.Equal(expectedValue, request.UserinfoSignedResponseAlg);

        Assert.Equal(expectedValue, request.IdTokenEncryptedResponseEnc);
        Assert.Equal(expectedValue, request.IdTokenEncryptedResponseAlg);
        Assert.Equal(expectedValue, request.IdTokenSignedResponseAlg);
    }

    [Theory]
    [InlineData("GET", "random_value", "random_value")]
    [InlineData("DELETE", "random_value", "random_value")]
    [InlineData("GET", "", "")]
    [InlineData("DELETE", "", "")]
    [InlineData("GET", null, null)]
    [InlineData("DELETE", null, null)]
    public async Task GetRequest_StringParametersGetAndDelete_ExpectValues(string method, string? value, string? expectedValue)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = method,
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    { Parameter.ClientId, value }
                })
            }
        };
        var serviceProvider = BuildServiceProvider(services =>
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var authResult = AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(), OAuthTokenAuthenticationDefaults.AuthenticationScheme));

            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = Parameter.AccessToken, Value = value }
            });

            authenticationServiceMock
                .Setup(x => x.AuthenticateAsync(httpContext, OAuthTokenAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(authResult);

            services.AddScopedMock(authenticationServiceMock);
        });
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RegisterRequest>>();
        httpContext.RequestServices = serviceProvider;

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(method, request.Method.Method);
        Assert.Equal(expectedValue, request.ClientId);
        Assert.Equal(expectedValue, request.RegistrationAccessToken);
    }

    [Theory]
    [InlineData("POST", true, true)]
    [InlineData("PUT", true, true)]
    [InlineData("POST", false, false)]
    [InlineData("PUT", false, false)]
    public async Task GetRequest_ValidBoolParametersPostAndPut_ExpectValues(string method, bool value, bool? expectedValue)
    {
        // Arrange
        var requestContent = new Dictionary<string, object>
        {
            { Parameter.RequireSignedRequestObject, value },
            { Parameter.RequireReferenceToken, value },
            { Parameter.RequirePushedAuthorizationRequests, value }
        };
        var requestJson = JsonSerializer.SerializeToUtf8Bytes(requestContent);
        var requestStream = new MemoryStream(requestJson);
        var queryContent = new Dictionary<string, StringValues>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = method,
                Body = requestStream,
                ContentLength = requestStream.Length,
                ContentType = MimeTypeConstants.Json,
                Query = new QueryCollection(queryContent)
            },
        };

        var serviceProvider = BuildServiceProvider(services =>
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var authResult = AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(), OAuthTokenAuthenticationDefaults.AuthenticationScheme));

            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = Parameter.AccessToken, Value = "value" }
            });

            authenticationServiceMock
                .Setup(x => x.AuthenticateAsync(httpContext, OAuthTokenAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(authResult);

            services.AddScopedMock(authenticationServiceMock);
        });
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RegisterRequest>>();
        httpContext.RequestServices = serviceProvider;

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(method, request.Method.Method);
        Assert.Equal(expectedValue, request.RequireSignedRequestObject);
        Assert.Equal(expectedValue, request.RequireReferenceToken);
        Assert.Equal(expectedValue, request.RequirePushedAuthorizationRequests);
    }

    [Theory]
    [InlineData("POST", 0, 0)]
    [InlineData("PUT", 0, 0)]
    [InlineData("POST", 10, 10)]
    [InlineData("PUT", 10, 10)]
    [InlineData("POST", -10, -10)]
    [InlineData("PUT", -10, -10)]
    public async Task GetRequest_IntParametersPostAndPut_ExpectValues(string method, int value, int? expectedValue)
    {
        // Arrange
        var requestContent = new Dictionary<string, int?>
        {
            { Parameter.AuthorizationCodeExpiration, value },
            { Parameter.AccessTokenExpiration, value },
            { Parameter.RefreshTokenExpiration, value },
            { Parameter.ClientSecretExpiration, value },
            { Parameter.JwksExpiration, value },
            { Parameter.RequestUriExpiration, value }
        };
        var requestJson = JsonSerializer.SerializeToUtf8Bytes(requestContent);
        var requestStream = new MemoryStream(requestJson);
        var queryContent = new Dictionary<string, StringValues>();
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = method,
                Body = requestStream,
                ContentLength = requestStream.Length,
                ContentType = MimeTypeConstants.Json,
                Query = new QueryCollection(queryContent)
            },
        };

        var serviceProvider = BuildServiceProvider(services =>
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var authResult = AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(), OAuthTokenAuthenticationDefaults.AuthenticationScheme));

            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = Parameter.AccessToken, Value = "value" }
            });

            authenticationServiceMock
                .Setup(x => x.AuthenticateAsync(httpContext, OAuthTokenAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(authResult);

            services.AddScopedMock(authenticationServiceMock);
        });
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RegisterRequest>>();
        httpContext.RequestServices = serviceProvider;

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(method, request.Method.Method);
        Assert.Equal(expectedValue, request.AuthorizationCodeExpiration);
        Assert.Equal(expectedValue, request.AccessTokenExpiration);
        Assert.Equal(expectedValue, request.RefreshTokenExpiration);
        Assert.Equal(expectedValue, request.ClientSecretExpiration);
        Assert.Equal(expectedValue, request.JwksExpiration);
        Assert.Equal(expectedValue, request.RequestUriExpiration);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task GetRequest_CollectionAndSpaceDelimitedParametersPostAndPut_ExpectValues(string method)
    {
        // Arrange
        const string spaceDelimitedValue = "three random values";
        string[] values = ["three", "random", "values"];
        string[] expectedValue = ["three", "random", "values"];
        var requestContent = new Dictionary<string, object?>
        {
            { Parameter.DefaultAcrValues, spaceDelimitedValue },
            { Parameter.Scope, spaceDelimitedValue },
            { Parameter.RedirectUris, values },
            { Parameter.PostLogoutRedirectUris, values },
            { Parameter.RequestUris, values },
            { Parameter.ResponseTypes, values },
            { Parameter.GrantTypes, values },
            { Parameter.Contacts, values }
        };
        var requestJson = JsonSerializer.SerializeToUtf8Bytes(requestContent);
        var requestStream = new MemoryStream(requestJson);
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = method,
                Body = requestStream,
                ContentLength = requestStream.Length,
                ContentType = MimeTypeConstants.Json
            }
        };
        var serviceProvider = BuildServiceProvider(services =>
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            services.AddScopedMock(authenticationServiceMock);
        });
        httpContext.RequestServices = serviceProvider;
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RegisterRequest>>();

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(method, request.Method.Method);        
        Assert.Equal(expectedValue, request.DefaultAcrValues);
        Assert.Equal(expectedValue, request.Scope);
        Assert.Equal(expectedValue, request.RedirectUris);
        Assert.Equal(expectedValue, request.PostLogoutRedirectUris);
        Assert.Equal(expectedValue, request.RequestUris);
        Assert.Equal(expectedValue, request.ResponseTypes);
        Assert.Equal(expectedValue, request.GrantTypes);
        Assert.Equal(expectedValue, request.Contacts);
    }

    [Theory]
    [InlineData("POST", "", 1)]
    [InlineData("PUT", "", 1)]
    [InlineData("POST", null, 0)]
    [InlineData("PUT", null, 0)]
    public async Task GetRequest_SpaceDelimitedParametersPostAndPut_ExpectZeroValues(string method, string? value, int expectedCount)
    {
        // Arrange
        var requestContent = new Dictionary<string, object?>
        {
            { Parameter.DefaultAcrValues, value },
            { Parameter.Scope, value }
        };
        var requestJson = JsonSerializer.SerializeToUtf8Bytes(requestContent);
        var requestStream = new MemoryStream(requestJson);
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = method,
                Body = requestStream,
                ContentLength = requestStream.Length,
                ContentType = MimeTypeConstants.Json
            }
        };
        var serviceProvider = BuildServiceProvider(services =>
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            services.AddScopedMock(authenticationServiceMock);
        });
        httpContext.RequestServices = serviceProvider;
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RegisterRequest>>();

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(method, request.Method.Method);
        Assert.Equal(expectedCount, request.DefaultAcrValues.Count);
        Assert.Equal(expectedCount, request.Scope.Count);
    }

    [Theory]
    [InlineData("POST", "", 0)]
    [InlineData("PUT", "", 0)]
    [InlineData("POST", "[]", 0)]
    [InlineData("PUT", "[]", 0)]
    [InlineData("POST", "invalid_json_array", 0)]
    [InlineData("PUT", "invalid_json_array", 0)]
    [InlineData("POST", null, 0)]
    [InlineData("PUT", null, 0)]
    public async Task GetRequest_CollectionParametersPostAndPut_ExpectZeroValues(string method, string? value, int expectedCount)
    {
        // Arrange
        var requestContent = new Dictionary<string, object?>
        {
            { Parameter.RedirectUris, value },
            { Parameter.PostLogoutRedirectUris, value },
            { Parameter.RequestUris, value },
            { Parameter.ResponseTypes, value },
            { Parameter.GrantTypes, value },
            { Parameter.Contacts, value }
        };
        var requestJson = JsonSerializer.SerializeToUtf8Bytes(requestContent);
        var requestStream = new MemoryStream(requestJson);
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = method,
                Body = requestStream,
                ContentLength = requestStream.Length,
                ContentType = MimeTypeConstants.Json
            }
        };
        var serviceProvider = BuildServiceProvider(services =>
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            services.AddScopedMock(authenticationServiceMock);
        });
        httpContext.RequestServices = serviceProvider;
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<RegisterRequest>>();

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(method, request.Method.Method);
        Assert.Equal(expectedCount, request.RedirectUris.Count);
        Assert.Equal(expectedCount, request.PostLogoutRedirectUris.Count);
        Assert.Equal(expectedCount, request.RequestUris.Count);
        Assert.Equal(expectedCount, request.ResponseTypes.Count);
        Assert.Equal(expectedCount, request.GrantTypes.Count);
        Assert.Equal(expectedCount, request.Contacts.Count);
    }
}