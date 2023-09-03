using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.IdToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.SilentTokenLogin;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class SilentTokenLoginHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var handler = serviceProvider.GetRequiredService<IRequestHandler<SilentTokenLoginCommand, SilentTokenLoginResponse>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var command = new SilentTokenLoginCommand
    {
      ClientId = client.Id,
      Nonce = CryptographyHelper.GetRandomString(16),
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      Scope = $"{ScopeConstants.OpenId}",
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      ResponseType = ResponseTypeConstants.Code,
      IdTokenHint = idToken
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }

  private async Task<Client> GetClient()
  {
    var grantType = await IdentityContext
      .Set<GrantType>()
      .SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);

    var openIdScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.OpenId);

    var consent = ConsentGrantBuilder
      .Instance()
      .AddScopes(new [] { openIdScope })
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5001/callback")
      .AddScope(openIdScope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretPost)
      .AddConsentGrant(consent)
      .Build();

    var nonce = NonceBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationCode = AuthorizationCodeBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddClient(client)
      .AddNonce(nonce)
      .AddAuthorizationCode(authorizationCode)
      .Build();

    var session = SessionBuilder
      .Instance()
      .AddAuthorizationCodeGrant(authorizationCodeGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddSession(session)
      .AddConsentGrant(consent)
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return client;
  }
}