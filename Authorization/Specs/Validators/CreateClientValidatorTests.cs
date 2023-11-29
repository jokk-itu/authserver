using System.Net;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Requests.CreateClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;
using Xunit;

namespace Specs.Validators;
public class CreateClientValidatorTests : BaseUnitTest
{
  private const string Jwks = @"
{
    ""keys"": [
        {
        ""kty"": ""RSA"",
        ""e"": ""AQAB"",
        ""use"": ""sig"",
        ""kid"": ""NTAxZmMxNDMyZDg3MTU1ZGM0MzEzODJhZWI4NDNlZDU1OGFkNjFiMQ"",
        ""alg"": ""RS256"",
        ""n"": ""luZFdW1ynitztkWLC6xKegbRWxky-5P0p4ShYEOkHs30QI2VCuR6Qo4Bz5rTgLBrky03W1GAVrZxuvKRGj9V9-PmjdGtau4CTXu9pLLcqnruaczoSdvBYA3lS9a7zgFU0-s6kMl2EhB-rk7gXluEep7lIOenzfl2f6IoTKa2fVgVd3YKiSGsyL4tztS70vmmX121qm0sTJdKWP4HxXyqK9neolXI9fYyHOYILVNZ69z_73OOVhkh_mvTmWZLM7GM6sApmyLX6OXUp8z0pkY-vT_9-zRxxQs7GurC4_C1nK3rI_0ySUgGEafO1atNjYmlFN-M3tZX6nEcA6g94IavyQ""
        }
    ]
}";

  private readonly CreateClientCommand _command = new()
  {
    ApplicationType = string.Empty,
    ResponseTypes = new List<string>(),
    TokenEndpointAuthMethod = string.Empty,
    Contacts = new List<string>(),
    PolicyUri = string.Empty,
    RedirectUris = new[] { "https://localhost:5002/callback" },
    Scope = string.Empty,
    GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
    SubjectType = string.Empty,
    TosUri = string.Empty,
    ClientName = "test",
    ClientUri = string.Empty,
    DefaultMaxAge = string.Empty,
    LogoUri = string.Empty,
    InitiateLoginUri = string.Empty,
    BackchannelLogoutUri = string.Empty,
    PostLogoutRedirectUris = new List<string>()
  };

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ForceDefaultValues_ExpectCreatedResult()
  {
    // Arrange
    _command.JwksUri = "https://localhost:5002/.well-known/jwks";
    var response = new HttpResponseMessage(HttpStatusCode.OK)
    {
      Content = new StringContent(Jwks)
    };

    var httpClientMock = new Mock<HttpClient>();
    httpClientMock
      .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(response)
      .Verifiable();

    var httpClientFactoryMock = new Mock<IHttpClientFactory>();
    httpClientFactoryMock
      .Setup(x => x.CreateClient(It.IsAny<string>()))
      .Returns(httpClientMock.Object)
      .Verifiable();

    var serviceProvider = BuildServiceProvider(services =>
    {
      services.AddSingletonMock(httpClientFactoryMock);
    });
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_FullCommand_ExpectCreatedResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scope = $"{ScopeConstants.OpenId}",
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test",
      ClientUri = "https://localhost:5002",
      DefaultMaxAge = "120",
      LogoUri = "https://gravatar.com/avatar",
      InitiateLoginUri = "https://localhost:5002/login",
      BackchannelLogoutUri = "https://localhost:5002/logout",
      PostLogoutRedirectUris = new[] { "https://localhost:5002" },
      Jwks = Jwks
    };
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidApplicationType_ExpectErrorResult()
  {
    // Arrange
    _command.ApplicationType = "wrong";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientName_ExpectErrorResult()
  {
    // Arrange
    _command.ClientName = string.Empty;
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExistingClientName_ExpectErrorResult()
  {
    // Arrange
    await IdentityContext
      .Set<Client>()
      .AddAsync(new Client
      {
        Id = Guid.NewGuid().ToString(),
        Name = "test"
      });
    await IdentityContext.SaveChangesAsync();

    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidRedirectUris_ExpectErrorResult()
  {
    // Arrange
    _command.RedirectUris = new[] { "invalid" };
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyRedirectUris_ExpectErrorResult()
  {
    // Arrange
    _command.RedirectUris = Array.Empty<string>();
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidResponseType_ExpectErrorResult()
  {
    // Arrange
    _command.ResponseTypes = new[] { "invalid" };
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidGrantTypes_ExpectErrorResult()
  {
    // Arrange
    _command.GrantTypes = new[] { "invalid" };
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyGrantTypes_ExpectErrorResult()
  {
    // Arrange
    _command.GrantTypes = Array.Empty<string>();
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidContacts_ExpectErrorResult()
  {
    // Arrange
    _command.Contacts = new[] { "invalid" };
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidScopes_ExpectErrorResult()
  {
    // Arrange
    _command.Scope = "invalid_scope";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidPolicy_ExpectErrorResult()
  {
    // Arrange
    _command.PolicyUri = "invalid";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidTos_ExpectErrorResult()
  {
    // Arrange
    _command.TosUri = "invalid";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidSubjectType_ExpectErrorResult()
  {
    // Arrange
    _command.SubjectType = "invalid";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidTokenEndpointAuthMethod_ExpectErrorResult()
  {
    // Arrange
    _command.TokenEndpointAuthMethod = "invalid";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidInitiateLoginUri_ExpectErrorResult()
  {
    // Arrange
    _command.InitiateLoginUri = "invalid_uri";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidLogoUri_ExpectErrorResult()
  {
    // Arrange
    _command.LogoUri = "invalid_uri";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientUri_ExpectErrorResult()
  {
    // Arrange
    _command.ClientUri = "invalid_uri";
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Theory]
  [InlineData("-2")]
  [InlineData("invalid_number")]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidDefaultMaxAge_ExpectErrorResult(string number)
  {
    // Arrange
    _command.DefaultMaxAge = number;
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidPostLogoutRedirectUris_ExpectErrorResult()
  {
    // Arrange
    _command.PostLogoutRedirectUris = new[] { "invalid_uri" };
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Theory]
  [InlineData("invalid_uri")]
  [InlineData("https://localhost:5002/callback#something")]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidBackChannelLogoutUri_ExpectErrorResult(string uri)
  {
    // Arrange
    _command.BackchannelLogoutUri = uri;
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidCombinationOfJwksAndJwksUri_ExpectErrorResult()
  {
    // Arrange
    _command.JwksUri = "uri";
    _command.Jwks = "jwks";

    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidJwks_ExpectErrorResult()
  {
    // Arrange
    var jwksUri = "https://localhost:5002/jwks";
    _command.Jwks = @"
{
    ""keys"": [
        {
        ""kty"": ""RSA"",
        ""e"": ""AQAB"",
        ""use"": ""sig"",
        ""kid"": ""NTAxZmMxNDMyZDg3MTU1ZGM0MzEzODJhZWI4NDNlZDU1OGFkNjFiMQ"",
        ""alg"": ""RS256"",
        ""n"": ""luZFdW1ynitztkWLC6xKegbRWxky-5P0p4ShYEOkHs30QI2VCuR6Qo4Bz5rTgLBrky03W1GAVrZxuvKRGj9V9-PmjdGtau4CTXu9pLLcqnruaczoSdvBYA3lS9a7zgFU0-s6kMl2EhB-rk7gXluEep7lIOenzfl2f6IoTKa2fVgVd3YKiSGsyL4tztS70vmmX121qm0sTJdKWP4HxXyqK9neolXI9fYyHOYILVNZ69z_73OOVhkh_mvTmWZLM7GM6sApmyLX6OXUp8z0pkY-vT_9-zRxxQs7GurC4_C1nK3rI_0ySUgGEafO1atNjYmlFN-M3tZX6nEcA6g94IavyQ""
        },
        {
        ""kty"": ""invalid"",
        ""e"": ""invalid"",
        ""use"": ""invalid"",
        ""alg"": ""invalid"",
        ""n"": ""invalid""
        }
    ]
}";

    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidJwksUri_ExpectErrorResult()
  {
    // Arrange
    var jwksUri = "https://localhost:5002/jwks";
    _command.JwksUri = jwksUri;

    var response = new HttpResponseMessage(HttpStatusCode.NotFound);

    var httpClientMock = new Mock<HttpClient>();
    httpClientMock
      .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(response)
      .Verifiable();

    var httpClientFactoryMock = new Mock<IHttpClientFactory>();
    httpClientFactoryMock
      .Setup(x => x.CreateClient(It.IsAny<string>()))
      .Returns(httpClientMock.Object)
      .Verifiable();

    var serviceProvider = BuildServiceProvider(services =>
    {
      services.AddSingletonMock(httpClientFactoryMock);
    });

    var validator = serviceProvider.GetRequiredService<IValidator<CreateClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }
}
