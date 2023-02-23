﻿using System.Net;
using Application;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemAuthorizationCodeGrant;
public class RedeemAuthorizationCodeGrantHandler : IRequestHandler<RedeemAuthorizationCodeGrantCommand, RedeemAuthorizationCodeGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<RedeemAuthorizationCodeGrantCommand> _validator;
  private readonly ITokenBuilder _tokenBuilder;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly ICodeDecoder _codeDecoder;

  public RedeemAuthorizationCodeGrantHandler(
    IdentityContext identityContext,
    IValidator<RedeemAuthorizationCodeGrantCommand> validator,
    ITokenBuilder tokenBuilder,
    IdentityConfiguration identityConfiguration,
    ICodeDecoder codeDecoder)
  {
    _identityContext = identityContext;
    _validator = validator;
    _tokenBuilder = tokenBuilder;
    _identityConfiguration = identityConfiguration;
    _codeDecoder = codeDecoder;
  }

  public async Task<RedeemAuthorizationCodeGrantResponse> Handle(RedeemAuthorizationCodeGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
    {
      return new RedeemAuthorizationCodeGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);
    }

    var code = _codeDecoder.DecodeAuthorizationCode(request.Code);

    var authorizationCodeGrant = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Include(x => x.Session)
      .ThenInclude(x => x.User)
      .SingleAsync(x => x.Id == code.AuthorizationGrantId, cancellationToken: cancellationToken);

    authorizationCodeGrant.IsCodeRedeemed = true;
    var sessionId = authorizationCodeGrant.Session.Id.ToString();
    var userId = authorizationCodeGrant.Session.User.Id;

    var accessToken = await _tokenBuilder.BuildAccessTokenAsync(request.ClientId, code.Scopes, userId, sessionId, cancellationToken: cancellationToken);
    var refreshToken = await _tokenBuilder.BuildRefreshTokenAsync(request.ClientId, code.Scopes, userId, sessionId, cancellationToken: cancellationToken);
    var idToken = await _tokenBuilder.BuildIdTokenAsync(request.ClientId, code.Scopes, authorizationCodeGrant.Nonce, userId, sessionId, authorizationCodeGrant.AuthTime, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new RedeemAuthorizationCodeGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }
}