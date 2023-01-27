using Application;
using Application.Validation;
using Infrastructure.Requests.RedeemClientCredentialsGrant;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using Domain.Constants;
using Infrastructure.Helpers;
using Xunit;

namespace Specs.Handlers;
public class RedeemClientCredentialsGrantHandlerTests : BaseUnitTest
{
  [Fact]
  public async Task Handle_Invalid()
  {
    // Arrange
    var validator = new Mock<IValidator<RedeemClientCredentialsGrantCommand>>();
    validator
      .Setup(x => 
        x.ValidateAsync(It.IsAny<RedeemClientCredentialsGrantCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult(ErrorCode.LoginRequired, "error", HttpStatusCode.Unauthorized));

    var serviceProvider = BuildServiceProvider(services => 
      services.AddTransient(_ => validator.Object));

    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientId = "test",
      ClientSecret = CryptographyHelper.GetRandomString(16),
      GrantType = GrantTypeConstants.ClientCredentials,
      Scope = "scope"
    };
    var handler = serviceProvider.GetRequiredService<IRequestHandler<RedeemClientCredentialsGrantCommand, RedeemClientCredentialsGrantResponse>>();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(response.IsError());
  }

  [Fact]
  public async Task Handle_Ok()
  {
    // Arrange
    var validator = new Mock<IValidator<RedeemClientCredentialsGrantCommand>>();
    validator
      .Setup(x => 
        x.ValidateAsync(It.IsAny<RedeemClientCredentialsGrantCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult(HttpStatusCode.OK));

    var serviceProvider = BuildServiceProvider(services => 
      services.AddTransient(_ => validator.Object));

    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientId = "test",
      ClientSecret = CryptographyHelper.GetRandomString(16),
      GrantType = GrantTypeConstants.ClientCredentials,
      Scope = "scope"
    };
    var handler = serviceProvider.GetRequiredService<IRequestHandler<RedeemClientCredentialsGrantCommand, RedeemClientCredentialsGrantResponse>>();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }
}