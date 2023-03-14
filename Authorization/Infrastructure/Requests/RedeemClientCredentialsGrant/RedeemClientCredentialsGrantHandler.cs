using System.Net;
using Application;
using Infrastructure.Builders.Abstractions;
using MediatR;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;
public class RedeemClientCredentialsGrantHandler : IRequestHandler<RedeemClientCredentialsGrantCommand, RedeemClientCredentialsGrantResponse>
{
  private readonly ITokenBuilder _tokenBuilder;
  private readonly IdentityConfiguration _identityConfiguration;

  public RedeemClientCredentialsGrantHandler(
    ITokenBuilder tokenBuilder,
    IdentityConfiguration identityConfiguration)
  {
    _tokenBuilder = tokenBuilder;
    _identityConfiguration = identityConfiguration;
  }

  public async Task<RedeemClientCredentialsGrantResponse> Handle(RedeemClientCredentialsGrantCommand request,
    CancellationToken cancellationToken)
  {
    var accessToken = await _tokenBuilder.BuildClientAccessToken(request.ClientId, request.Scope.Split(' '),
      cancellationToken: cancellationToken);
    return new RedeemClientCredentialsGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }
}