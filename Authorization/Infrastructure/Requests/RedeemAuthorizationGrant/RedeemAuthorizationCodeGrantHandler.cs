using System.Net;
using Application.Validation;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Factories;
using MediatR;

namespace Infrastructure.Requests.CreateAuthorizationCodeGrant;
public class RedeemAuthorizationCodeGrantHandler : IRequestHandler<RedeemAuthorizationCodeGrantCommand, RedeemAuthorizationCodeGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<RedeemAuthorizationCodeGrantCommand> _validator;
  private readonly CodeFactory _codeFactory;
  private readonly ITokenBuilder _tokenBuilder;
  private readonly IdentityConfiguration _identityConfiguration;

  public RedeemAuthorizationCodeGrantHandler(
    IdentityContext identityContext,
    IValidator<RedeemAuthorizationCodeGrantCommand> validator,
    CodeFactory codeFactory,
    ITokenBuilder tokenBuilder,
    IdentityConfiguration identityConfiguration)
  {
    _identityContext = identityContext;
    _validator = validator;
    _codeFactory = codeFactory;
    _tokenBuilder = tokenBuilder;
    _identityConfiguration = identityConfiguration;
  }

  public async Task<RedeemAuthorizationCodeGrantResponse> Handle(RedeemAuthorizationCodeGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new RedeemAuthorizationCodeGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var scopes = request.Scope.Split(' ');
    var code = _codeFactory.DecodeCode(request.Code);
    if (code is null)
      throw new Exception("code is not decodable after validation");

    var accessToken = await _tokenBuilder.BuildAccessTokenAsync(request.ClientId, scopes, code.UserId, cancellationToken: cancellationToken);
    var refreshToken = await _tokenBuilder.BuildRefreshTokenAsync(request.ClientId, scopes, code.UserId, cancellationToken: cancellationToken);
    var idToken = _tokenBuilder.BuildIdToken(request.ClientId, scopes, code.Nonce, code.UserId);
    return new RedeemAuthorizationCodeGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }
}
