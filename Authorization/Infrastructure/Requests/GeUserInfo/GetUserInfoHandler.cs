using System.Globalization;
using System.Net;
using System.Security;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GeUserInfo;
public class GetUserInfoHandler : IRequestHandler<GetUserInfoQuery, GetUserInfoResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly UserManager<User> _userManager;
  private readonly IValidator<GetUserInfoQuery> _validator;
  private readonly ITokenDecoder _tokenDecoder;

  public GetUserInfoHandler(
    IdentityContext identityContext,
    UserManager<User> userManager,
    IValidator<GetUserInfoQuery> validator,
    ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _userManager = userManager;
    _validator = validator;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<GetUserInfoResponse> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validationResult.IsError())
      return new GetUserInfoResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var token = _tokenDecoder.DecodeSignedToken(request.AccessToken);
    if (token is null)
      throw new SecurityException("token must be valid");

    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var clientId = token.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value;
    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Where(cg => cg.User.Id == userId)
      .Where(cg => cg.Client.Id == clientId)
      .Include(cg => cg.ConsentedClaims)
      .SingleAsync(cancellationToken: cancellationToken);

    var userInfo = new List<(string, string)>();
    foreach (var claim in consentGrant.ConsentedClaims)
    {
      switch (claim.Name)
      {
        case ClaimNameConstants.Name:
          userInfo.Add((claim.Name, consentGrant.User.GetName()));
          break;
        case ClaimNameConstants.GivenName:
          userInfo.Add((claim.Name, consentGrant.User.FirstName));
          break;
        case ClaimNameConstants.FamilyName:
          userInfo.Add((claim.Name, consentGrant.User.LastName));
          break;
        case ClaimNameConstants.Address:
          var roles = await _userManager.GetRolesAsync(consentGrant.User);
          userInfo.AddRange(roles.Select(role => (claim.Name, role)));
          break;
        case ClaimNameConstants.Birthdate:
          userInfo.Add((claim.Name, consentGrant.User.Birthdate.ToString(CultureInfo.InvariantCulture)));
          break;
        case ClaimNameConstants.Locale:
          userInfo.Add((claim.Name, consentGrant.User.Locale));
          break;
        case ClaimNameConstants.Phone:
          userInfo.Add((claim.Name, consentGrant.User.PhoneNumber));
          break;
        case ClaimNameConstants.Email:
          userInfo.Add((claim.Name, consentGrant.User.Email));
          break;
      }
    }
    return new GetUserInfoResponse(HttpStatusCode.OK)
    {
      UserInfo = userInfo
    };
  }
}
