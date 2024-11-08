using AuthServer.Core.Request;
using AuthServer.RequestAccessors.Token;
using AuthServer.TokenByGrant;
using AuthServer.TokenByGrant.RefreshTokenGrant;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RefreshTokenGrant;

public class RefreshTokenRequestValidatorTest : BaseUnitTest
{
    public RefreshTokenRequestValidatorTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Validate_EmptyGrantType_ExpectUnsupportedGrantType()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var refreshTokenRequestValidator = serviceProvider
            .GetRequiredService<IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>>();

        var request = new TokenRequest();

        // Act
        var processResult = await refreshTokenRequestValidator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(TokenError.UnsupportedGrantType, processResult);
    }
}