using System.Net;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Services.Abstract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GetUserInfo;
public class GetUserInfoHandler : IRequestHandler<GetUserInfoQuery, GetUserInfoResponse>
{
  private readonly IStructuredTokenDecoder _tokenDecoder;
  private readonly IClaimService _claimService;
  private readonly IdentityContext _identityContext;

  public GetUserInfoHandler(
    IStructuredTokenDecoder tokenDecoder,
    IClaimService claimService,
    IdentityContext identityContext)
  {
    _tokenDecoder = tokenDecoder;
    _claimService = claimService;
    _identityContext = identityContext;
  }

  public async Task<GetUserInfoResponse> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
  {
    string authorizationGrantId;
    if (TokenHelper.IsStructuredToken(request.AccessToken))
    {
      var token = await _tokenDecoder.Decode(request.AccessToken, new StructuredTokenDecoderArguments
      {
        ValidateAudience = false,
        ValidateLifetime = false
      });
      authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
    }
    else
    {
      authorizationGrantId = await _identityContext
        .Set<GrantToken>()
        .Where(x => x.Reference == request.AccessToken)
        .Select(x => x.AuthorizationGrant.Id)
        .SingleAsync(cancellationToken: cancellationToken);
    }

    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == authorizationGrantId)
      .Select(x => new
      {
        UserId = x.Session.User.Id,
        ClientId = x.Client.Id
      })
      .SingleAsync(cancellationToken: cancellationToken);

    var userInfo = await _claimService.GetClaimsFromConsentGrant(query.UserId, query.ClientId, cancellationToken: cancellationToken);
    return new GetUserInfoResponse(HttpStatusCode.OK)
    {
      UserInfo = userInfo
    };
  }
}
