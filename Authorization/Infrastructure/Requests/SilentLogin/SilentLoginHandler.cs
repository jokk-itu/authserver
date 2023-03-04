using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentLogin;
public class SilentLoginHandler : IRequestHandler<SilentLoginCommand, SilentLoginResponse>
{
  private readonly IValidator<SilentLoginCommand> _validator;
  private readonly ITokenDecoder _tokenDecoder;
  private readonly IdentityContext _identityContext;
  private readonly ICodeBuilder _codeBuilder;

  public SilentLoginHandler(
    IValidator<SilentLoginCommand> validator,
    ITokenDecoder tokenDecoder,
    IdentityContext identityContext,
    ICodeBuilder codeBuilder)
  {
    _validator = validator;
    _tokenDecoder = tokenDecoder;
    _identityContext = identityContext;
    _codeBuilder = codeBuilder;
  }

  public async Task<SilentLoginResponse> Handle(SilentLoginCommand request, CancellationToken cancellationToken)
  {
    var validatorResponse = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validatorResponse.IsError())
    {
      return new SilentLoginResponse(validatorResponse.ErrorCode, validatorResponse.ErrorDescription, validatorResponse.StatusCode);
    }

    var token = _tokenDecoder.DecodeSignedToken(request.IdTokenHint);
    if (token is null)
    {
      return new SilentLoginResponse(ErrorCode.ServerError, "something went wrong", HttpStatusCode.OK);
    }

    var authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
    var authorizationCodeId = Guid.NewGuid().ToString();
    var nonceId = Guid.NewGuid().ToString();
    var scopes = request.Scope.Split(' ');

    var authorizationGrant = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .SingleAsync(x => x.Id == authorizationGrantId, cancellationToken: cancellationToken);

    var code = await _codeBuilder.BuildAuthorizationCodeAsync(
      authorizationGrantId,
      authorizationCodeId,
      nonceId,
      request.CodeChallenge,
      request.CodeChallengeMethod,
      scopes);

    var authorizationCode = new AuthorizationCode
    {
      Id = authorizationCodeId,
      IssuedAt = DateTime.UtcNow,
      Value = code
    };

    var nonce = new Nonce
    {
      Id = nonceId,
      Value = request.Nonce
    };

    authorizationGrant.AuthorizationCodes.Add(authorizationCode);
    authorizationGrant.Nonces.Add(nonce);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new SilentLoginResponse(HttpStatusCode.OK)
    {
      Code = code
    };
  }
}