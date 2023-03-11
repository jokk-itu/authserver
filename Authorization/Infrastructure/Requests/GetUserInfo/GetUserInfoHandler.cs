using System.Net;
using System.Security;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Services.Abstract;
using MediatR;

namespace Infrastructure.Requests.GetUserInfo;
public class GetUserInfoHandler : IRequestHandler<GetUserInfoQuery, GetUserInfoResponse>
{
  private readonly ITokenDecoder _tokenDecoder;
  private readonly IClaimService _claimService;

  public GetUserInfoHandler(
    ITokenDecoder tokenDecoder,
    IClaimService claimService)
  {
    _tokenDecoder = tokenDecoder;
    _claimService = claimService;
  }

  public async Task<GetUserInfoResponse> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
  {
    var token = _tokenDecoder.DecodeSignedToken(request.AccessToken);
    if (token is null)
    {
      throw new SecurityException("token must be valid");
    }

    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var clientId = token.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value;
    var userInfo = await _claimService.GetClaimsFromConsentGrant(userId, clientId, cancellationToken: cancellationToken);
    return new GetUserInfoResponse(HttpStatusCode.OK)
    {
      UserInfo = userInfo
    };
  }
}
