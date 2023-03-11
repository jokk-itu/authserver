using System.Net;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.RedeemAuthorizationCodeGrant;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class RedeemAuthorizationGrantCodeHandlerTests : BaseUnitTest
{
  [Fact]
  public async Task Handle_Ok()
  {
    // Arrange
    var validator = new Mock<IValidator<RedeemAuthorizationCodeGrantCommand>>();
    var result = new ValidationResult(HttpStatusCode.OK);
    validator.Setup(x => x.ValidateAsync(It.IsAny<RedeemAuthorizationCodeGrantCommand>(), CancellationToken.None))
        .ReturnsAsync(result);

    var serviceProvider = BuildServiceProvider(services =>
    {
      services.AddTransient(_ => validator.Object);
    });

    var handler = serviceProvider
      .GetRequiredService<IRequestHandler<RedeemAuthorizationCodeGrantCommand, RedeemAuthorizationCodeGrantResponse>>();

    const string uri = "https://localhost:5000/callback";
    var client = ClientBuilder
      .Instance()
      .AddRedirect(new RedirectUri{Uri = uri})
      .Build();

    var nonceId = Guid.NewGuid().ToString();
    var authorizationCodeId = Guid.NewGuid().ToString();
    var authorizationCodeGrantId = Guid.NewGuid().ToString();
    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var code = await serviceProvider
      .GetRequiredService<ICodeBuilder>()
      .BuildAuthorizationCodeAsync(
        authorizationCodeGrantId,
        authorizationCodeId,
        nonceId,
        pkce.CodeChallenge,
        CodeChallengeMethodConstants.S256,
        new[] { ScopeConstants.OpenId });

    var nonce = NonceBuilder.Instance(nonceId).Build();
    var authorizationCode = AuthorizationCodeBuilder.Instance(authorizationCodeId).AddCode(code).Build();
    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance(authorizationCodeGrantId)
      .AddClient(client)
      .AddNonce(nonce)
      .AddAuthorizationCode(authorizationCode)
      .Build();

    authorizationCode.Value = code;

    var session = SessionBuilder
        .Instance()
        .AddAuthorizationCodeGrant(authorizationCodeGrant)
        .Build();

    var user = UserBuilder
        .Instance()
        .AddPassword(CryptographyHelper.GetRandomString(16))
        .AddSession(session)
        .Build();

    await IdentityContext
      .Set<User>()
      .AddAsync(user);

    await IdentityContext.SaveChangesAsync();

    var command = new RedeemAuthorizationCodeGrantCommand
    {
      ClientId = client.Id,
      ClientSecret = client.Secret,
      RedirectUri = uri,
      GrantType = GrantTypeConstants.AuthorizationCode,
      Scope = ScopeConstants.OpenId,
      Code = code,
      CodeVerifier = pkce.CodeVerifier
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }
}
