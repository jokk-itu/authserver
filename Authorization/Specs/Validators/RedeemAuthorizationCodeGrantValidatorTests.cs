using Application;
using Application.Validation;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.RedeemAuthorizationGrantCode;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers;
using Xunit;

namespace Specs.Validators;
public class RedeemAuthorizationCodeGrantValidatorTests : BaseUnitTest
{
    // Invalid Scope
    // Invalid GrantType
    // Invalid AuthorizationCodeGrant
    // Invalid Client
    // Unauthorized Client
    // Invalid Session

    [Fact]
    public async Task ValidateAsync_ExpectInvalidCodeVerifier()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var codeBuilder = serviceProvider.GetRequiredService<ICodeBuilder>();
        var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
        var code = await codeBuilder.BuildAuthorizationCodeAsync("123", pkce.CodeChallenge, CodeChallengeMethodConstants.S256, new[] {ScopeConstants.OpenId});
        var validator = serviceProvider.GetRequiredService<IValidator<RedeemAuthorizationCodeGrantCommand>>();
        var command = new RedeemAuthorizationCodeGrantCommand
        {
            Code = code,
            CodeVerifier = CryptographyHelper.GetRandomString(2)
        };

        // Act
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    }
}