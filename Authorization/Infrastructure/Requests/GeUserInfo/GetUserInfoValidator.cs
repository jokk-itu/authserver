﻿using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GeUserInfo;
public class GetUserInfoValidator : IValidator<GetUserInfoQuery>
{
  private readonly ITokenDecoder _tokenDecoder;
  private readonly IdentityContext _identityContext;
  private readonly UserManager<User> _userManager;

  public GetUserInfoValidator(
    ITokenDecoder tokenDecoder,
    IdentityContext identityContext,
    UserManager<User> userManager)
  {
    _tokenDecoder = tokenDecoder;
    _identityContext = identityContext;
    _userManager = userManager;
  }

  public async Task<ValidationResult> ValidateAsync(GetUserInfoQuery value, CancellationToken cancellationToken = default)
  {
    var token = _tokenDecoder.DecodeSignedToken(value.AccessToken);
    if (token is null)
      return new ValidationResult(ErrorCode.InvalidRequest, "access_token is invalid", HttpStatusCode.BadRequest);

    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var clientId = token.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value;
    var isUserValid = await _identityContext
      .Set<ConsentGrant>()
      .Where(cg => cg.Client.Id == clientId)
      .Where(cg => cg.User.Id == userId)
      .Select(cg => cg.User)
      .Select(u => u.Session)
      .Where(Session.IsValid)
      .AnyAsync(cancellationToken: cancellationToken);

    if(!isUserValid)
      return new ValidationResult(ErrorCode.AccessDenied, "session is invalid", HttpStatusCode.BadRequest);

    return new ValidationResult(HttpStatusCode.OK);
  }
}