using System.Security.Claims;
using AuthServer.Authentication.OAuthToken;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Userinfo;
using AuthServer.Tests.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RequestAccessors;

public class UserinfoRequestAccessorTest : BaseUnitTest
{
    public UserinfoRequestAccessorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task GetRequest_NormalStringParameters_ExpectValues()
    {
        // Arrange
        var token = CryptographyHelper.GetRandomString(32);
        var httpContext = new DefaultHttpContext();
        var serviceProvider = BuildServiceProvider(services =>
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var authResult = AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(), OAuthTokenAuthenticationDefaults.AuthenticationScheme));

            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = Parameter.AccessToken, Value = token }
            });

            authenticationServiceMock
                .Setup(x => x.AuthenticateAsync(httpContext, OAuthTokenAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(authResult);

            services.AddScopedMock(authenticationServiceMock);
        });
        var requestAccessor = serviceProvider.GetRequiredService<IRequestAccessor<UserinfoRequest>>();

        httpContext.RequestServices = serviceProvider;

        // Act
        var request = await requestAccessor.GetRequest(httpContext.Request);

        // Assert
        Assert.Equal(token, request.AccessToken);
    }
}
