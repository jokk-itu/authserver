using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Application;
using Application.Validation;
using Domain.Constants;
using Infrastructure.Decoders;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.SilentLogin;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Specs.Handlers;
public class SilentLoginHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ExpectInvalidValidation()
  {
    // Arrange
    var validationResult = new ValidationResult(ErrorCode.InvalidRequest, "invalid request", HttpStatusCode.BadRequest);
    var validator = new Mock<IValidator<SilentLoginQuery>>();
    validator
      .Setup(x => x.ValidateAsync(It.IsAny<SilentLoginQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(validationResult)
      .Verifiable();

    var serviceProvider = BuildServiceProvider(services =>
    {
      services.AddScopedMock(validator);
    });

    var handler = serviceProvider.GetRequiredService<IRequestHandler<SilentLoginQuery, SilentLoginResponse>>();

    // Act
    var response = await handler.Handle(new SilentLoginQuery(), CancellationToken.None);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(validationResult.ErrorCode, response.ErrorCode);
    Assert.Equal(validationResult.ErrorDescription, response.ErrorDescription);
    validator.Verify();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_Ok()
  {
    // Arrange
    var validationResult = new ValidationResult(HttpStatusCode.OK);
    var validator = new Mock<IValidator<SilentLoginQuery>>();
    validator
      .Setup(x => x.ValidateAsync(It.IsAny<SilentLoginQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(validationResult)
      .Verifiable();

    var token = new JwtSecurityToken(claims: new[] { new Claim(ClaimNameConstants.Sub, "userId") });
    var tokenDecoder = new Mock<ITokenDecoder>();
    tokenDecoder
      .Setup(x => x.DecodeSignedToken(It.IsAny<string>()))
      .Returns(token)
      .Verifiable();

    var serviceProvider = BuildServiceProvider(services =>
    {
      services.AddScopedMock(validator);
      services.AddScopedMock(tokenDecoder);
    });

    var handler = serviceProvider.GetRequiredService<IRequestHandler<SilentLoginQuery, SilentLoginResponse>>();

    // Act
    var response = await handler.Handle(new SilentLoginQuery(), CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(token.Subject, response.UserId);
    validator.Verify();
  }
}