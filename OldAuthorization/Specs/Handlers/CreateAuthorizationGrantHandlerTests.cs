using System.Net;
using Domain.Constants;
using Infrastructure.Helpers;
using Infrastructure.Requests.CreateAuthorizationGrant;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class CreateAuthorizationGrantHandlerTests : BaseUnitTest
{
  [Fact]
  public async Task Handle_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var session = SessionBuilder
      .Instance()
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddSession(session)
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddRedirectUri("https://localhost:5000")
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    var handler = serviceProvider
      .GetRequiredService<IRequestHandler<CreateAuthorizationGrantCommand, CreateAuthorizationGrantResponse>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      Scope = $"{ScopeConstants.OpenId}",
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      RedirectUri = "https://localhost:5000",
      MaxAge = "20",
      Nonce = CryptographyHelper.GetRandomString(16),
      ResponseType = ResponseTypeConstants.Code,
      State = CryptographyHelper.GetRandomString(16),
      UserId = user.Id
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
    Assert.NotNull(response.Code);
    Assert.Equal(command.State, response.State);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}