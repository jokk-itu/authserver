using System.Net;
using Application;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.ClientAccessToken;
using Infrastructure.Repositories;
using MediatR;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;
public class RedeemClientCredentialsGrantHandler : IRequestHandler<RedeemClientCredentialsGrantCommand, RedeemClientCredentialsGrantResponse>
{
  private readonly ITokenBuilder<ClientAccessTokenArguments> _tokenBuilder;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly ResourceManager _resourceManager;

  public RedeemClientCredentialsGrantHandler(
    ITokenBuilder<ClientAccessTokenArguments> tokenBuilder,
    IdentityConfiguration identityConfiguration,
    ResourceManager resourceManager)
  {
    _tokenBuilder = tokenBuilder;
    _identityConfiguration = identityConfiguration;
    _resourceManager = resourceManager;
  }

  public async Task<RedeemClientCredentialsGrantResponse> Handle(RedeemClientCredentialsGrantCommand request,
    CancellationToken cancellationToken)
  {
    var scope = request.Scope.Split(' ');
    var resources = await _resourceManager.ReadResourcesAsync(scope, cancellationToken: cancellationToken);
    var accessToken = await _tokenBuilder.BuildToken(new ClientAccessTokenArguments
      {
        ClientId = request.ClientId,
        ResourceNames = resources.Select(x => x.Name),
        Scope = request.Scope
      });
    return new RedeemClientCredentialsGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }
}