using System.Net;
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

    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == code.AuthorizationGrantId)
      .Select(x => new
      {
        AuthorizationCodeGrant = x,
        SessionId = x.Session.Id.ToString(),
        UserId = x.Session.User.Id,
        AuthorizationCode = x.AuthorizationCodes.Single(y => y.Id == code.AuthorizationCodeId),
        Nonce = x.Nonces.Single(y => y.Id == code.NonceId)
      })
      .SingleAsync(cancellationToken: cancellationToken);

    query.AuthorizationCode.IsRedeemed = true;
    query.AuthorizationCode.RedeemedAt = DateTime.UtcNow;

    var accessToken = await _tokenBuilder.BuildAccessTokenAsync(request.ClientId, code.Scopes, query.UserId, query.SessionId, cancellationToken: cancellationToken);
    var refreshToken = await _tokenBuilder.BuildRefreshTokenAsync(code.AuthorizationGrantId, request.ClientId, code.Scopes, query.UserId, query.SessionId, cancellationToken: cancellationToken);
    var idToken = await _tokenBuilder.BuildIdTokenAsync(query.AuthorizationCodeGrant.Id, request.ClientId, code.Scopes, query.Nonce.Value, query.UserId, query.SessionId, query.AuthorizationCodeGrant.AuthTime, cancellationToken: cancellationToken);

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
