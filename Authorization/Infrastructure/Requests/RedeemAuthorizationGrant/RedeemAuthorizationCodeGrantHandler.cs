using System.Net;
using Application;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.CreateAuthorizationCodeGrant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemAuthorizationGrant;
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
      return new RedeemAuthorizationCodeGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var code = _codeDecoder.DecodeAuthorizationCode(request.Code);

    var authorizationCodeGrant = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Include(x => x.Session)
      .SingleAsync(x => x.Id == code.AuthorizationGrantId, cancellationToken: cancellationToken);

    authorizationCodeGrant.IsRedeemed = true;
    var sessionId = authorizationCodeGrant.Session.Id.ToString();

    var accessToken = await _tokenBuilder.BuildAccessTokenAsync(request.ClientId, code.Scopes, code.UserId, sessionId, cancellationToken: cancellationToken);
    var refreshToken = await _tokenBuilder.BuildRefreshTokenAsync(request.ClientId, code.Scopes, code.UserId, sessionId, cancellationToken: cancellationToken);
    var idToken = await _tokenBuilder.BuildIdTokenAsync(request.ClientId, code.Scopes, authorizationCodeGrant.Nonce, code.UserId, sessionId, authorizationCodeGrant.AuthTime, cancellationToken: cancellationToken);

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
