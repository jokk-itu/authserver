using System.Net;
using Application;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.ClientAccessToken;
using MediatR;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;
public class RedeemClientCredentialsGrantHandler : IRequestHandler<RedeemClientCredentialsGrantCommand, RedeemClientCredentialsGrantResponse>
{
  private readonly ITokenBuilder<ClientAccessTokenArguments> _tokenBuilder;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly IdentityContext _identityContext;

  public RedeemClientCredentialsGrantHandler(
    ITokenBuilder<ClientAccessTokenArguments> tokenBuilder,
    IdentityConfiguration identityConfiguration,
    IdentityContext identityContext)
  {
    _tokenBuilder = tokenBuilder;
    _identityConfiguration = identityConfiguration;
    _identityContext = identityContext;
  }

  public async Task<RedeemClientCredentialsGrantResponse> Handle(RedeemClientCredentialsGrantCommand request,
    CancellationToken cancellationToken)
  {
    var accessToken = await _tokenBuilder.BuildToken(new ClientAccessTokenArguments
      {
        ClientId = request.ClientId,
        Resource = request.Resource,
        Scope = request.Scope
      });
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new RedeemClientCredentialsGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }
}