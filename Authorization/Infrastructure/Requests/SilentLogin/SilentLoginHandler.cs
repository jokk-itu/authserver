using System.Net;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentLogin;
public class SilentLoginHandler : IRequestHandler<SilentLoginCommand, SilentLoginResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ICodeBuilder _codeBuilder;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public SilentLoginHandler(
    IdentityContext identityContext,
    ICodeBuilder codeBuilder,
    IStructuredTokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _codeBuilder = codeBuilder;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<SilentLoginResponse> Handle(SilentLoginCommand request, CancellationToken cancellationToken)
  {
    var token = await _tokenDecoder.Decode(request.IdTokenHint, new StructuredTokenDecoderArguments
    {
      ClientId = request.ClientId
    });

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