using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GetUserInfo;
public class GetUserInfoValidator : IValidator<GetUserInfoQuery>
{
  private readonly IStructuredTokenDecoder _tokenDecoder;
  private readonly IdentityContext _identityContext;

  public GetUserInfoValidator(
    IStructuredTokenDecoder tokenDecoder,
    IdentityContext identityContext)
  {
    _tokenDecoder = tokenDecoder;
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(GetUserInfoQuery value, CancellationToken cancellationToken = default)
  {
    string authorizationGrantId;
    if (TokenHelper.IsStructuredToken(value.AccessToken))
    {
      var token = await _tokenDecoder.Decode(value.AccessToken, new StructuredTokenDecoderArguments
      {
        ValidateAudience = false,
        ValidateLifetime = true
      });
      authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
    }
    else
    {
      authorizationGrantId = await _identityContext
        .Set<GrantToken>()
        .Where(x => x.Reference == value.AccessToken)
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


    var isUserValid = await _identityContext
      .Set<ConsentGrant>()
      .Where(cg => cg.Client.Id == query.ClientId)
      .Where(cg => cg.User.Id == query.UserId)
      .Select(cg => cg.User)
      .SelectMany(u => u.Sessions)
      .Where(x => !x.IsRevoked)
      .SelectMany(s => s.AuthorizationCodeGrants)
      .Where(g => g.Client.Id == query.ClientId)
      .Where(AuthorizationCodeGrant.IsMaxAgeValid)
      .AnyAsync(cancellationToken: cancellationToken);

    if (!isUserValid)
    {
      return new ValidationResult(ErrorCode.AccessDenied, "session is invalid", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}
