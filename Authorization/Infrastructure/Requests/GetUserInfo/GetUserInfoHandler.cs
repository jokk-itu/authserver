using System.Net;
using System.Security;
using Application.Validation;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.GetUserInfo;
using Infrastructure.Services.Abstract;
using MediatR;

namespace Infrastructure.Requests.GetUserInfo;
public class GetUserInfoHandler : IRequestHandler<GetUserInfoQuery, GetUserInfoResponse>
{
  private readonly IValidator<GetUserInfoQuery> _validator;
  private readonly ITokenDecoder _tokenDecoder;
  private readonly IClaimService _claimService;

  public GetUserInfoHandler(
    IValidator<GetUserInfoQuery> validator,
    ITokenDecoder tokenDecoder,
    IClaimService claimService)
  {
    _validator = validator;
    _tokenDecoder = tokenDecoder;
    _claimService = claimService;
  }

  public async Task<GetUserInfoResponse> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validationResult.IsError())
    {
      return new GetUserInfoResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);
    }

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
