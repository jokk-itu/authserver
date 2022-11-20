using Application;
using Domain.Constants;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders;
using Infrastructure.Decoders;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.DeleteClient;
using Xunit;
using Infrastructure.Helpers;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Specs.Handlers;

public class DeleteClientHandlerTests
{
  private readonly IdentityContext _identityContext;

  public DeleteClientHandlerTests()
  {
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<IdentityContext>()
      .UseSqlite(connection)
      .Options;
    _identityContext = new IdentityContext(options);
    _identityContext.Database.EnsureCreated();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ValidateFalse_ExpectErrorResponse()
  {
    // Arrange
    var fakeTokenDecoder = new Mock<ITokenDecoder>();
    var fakeValidator = new Mock<IValidator<DeleteClientCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.BadRequest)
    {
      ErrorCode = ErrorCode.InvalidClientMetadata,
      ErrorDescription = string.Empty
    };
    fakeValidator
      .Setup(x => x.ValidateAsync(It.IsAny<DeleteClientCommand>(), CancellationToken.None))
      .ReturnsAsync(validationResult);

    var handler = new DeleteClientHandler(_identityContext, fakeValidator.Object, fakeTokenDecoder.Object);
    var command = new DeleteClientCommand();

    // Act
    var errorResponse = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(errorResponse.ErrorCode, validationResult.ErrorCode);
    Assert.Equal(errorResponse.ErrorDescription, validationResult.ErrorDescription);
    Assert.Equal(errorResponse.StatusCode, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_CreateClient_ExpectCreatedResult()
  {
    // Arrange
    var client = new Client
    {
      Id = Guid.NewGuid().ToString(),
      Name = "test"
    };
    await _identityContext
      .Set<Client>()
      .AddAsync(client);
    await _identityContext.SaveChangesAsync();
    var identityConfiguration = new IdentityConfiguration
    {
      PrivateKeySecret = CryptographyHelper.GetRandomString(32),
      InternalIssuer = "auth-server"
    };
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var jwkManager = new JwkManager(serviceProvider);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager, identityConfiguration);
    var token = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager).BuildClientRegistrationAccessToken(client.Id);
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = token
    };
    var fakeValidator = new Mock<IValidator<DeleteClientCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.OK);
    fakeValidator
      .Setup(x => x.ValidateAsync(It.IsAny<DeleteClientCommand>(), CancellationToken.None))
      .ReturnsAsync(validationResult);

    var handler = new DeleteClientHandler(_identityContext, fakeValidator.Object, tokenDecoder);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }
}