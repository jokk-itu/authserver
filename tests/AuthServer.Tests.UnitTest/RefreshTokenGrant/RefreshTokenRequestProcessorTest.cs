using AuthServer.Tests.Core;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.RefreshTokenGrant;

public class RefreshTokenRequestProcessorTest : BaseUnitTest
{
    public RefreshTokenRequestProcessorTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    public async Task Process_ProcessRequest_ExpectTokenResponse()
    {
        // Arrange
        var accessTokenBuilder = new Mock<ITokenBuilder<GrantAccessTokenArguments>>();
        var idTokenBuilder = new Mock<ITokenBuilder<IdTokenArguments>>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(accessTokenBuilder);
            services.AddScopedMock(idTokenBuilder);
        });


    }
}