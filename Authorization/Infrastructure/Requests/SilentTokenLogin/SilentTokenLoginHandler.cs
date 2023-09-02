using System.Net;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Services;
using Infrastructure.Services.Abstract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentTokenLogin;
public class SilentTokenLoginHandler : IRequestHandler<SilentTokenLoginCommand, SilentTokenLoginResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IAuthorizationGrantService _authorizationGrantService;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public SilentTokenLoginHandler(
    IdentityContext identityContext,
    IAuthorizationGrantService authorizationGrantService,
    IStructuredTokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _authorizationGrantService = authorizationGrantService;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<SilentTokenLoginResponse> Handle(SilentTokenLoginCommand request, CancellationToken cancellationToken)
  {
    var token = await _tokenDecoder.Decode(request.IdTokenHint, new StructuredTokenDecoderArguments
    {
      ClientId = request.ClientId
    });

    var authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var authorizationGrant = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(AuthorizationCodeGrant.IsMaxAgeValid)
      .SingleOrDefaultAsync(x => x.Id == authorizationGrantId,
        cancellationToken: cancellationToken);

    string code;
    if (authorizationGrant is null)
    {
      code = await CreateGrant(request, userId, cancellationToken);
    }
    else
    {
      code = await UpdateGrant(authorizationGrant, request, cancellationToken);
    }

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new SilentTokenLoginResponse(HttpStatusCode.OK)
    {
      Code = code
    };
  }

  private async Task<string> UpdateGrant(
    AuthorizationCodeGrant authorizationCodeGrant,
    SilentTokenLoginCommand request,
    CancellationToken cancellationToken)
  {
    var result = await _authorizationGrantService.UpdateAuthorizationGrant(
      new UpdateAuthorizationGrantArguments
      {
        AuthorizationCodeGrant = authorizationCodeGrant,
        Nonce = request.Nonce,
        Scope = request.Scope,
        CodeChallenge = request.CodeChallenge,
        CodeChallengeMethod = request.CodeChallengeMethod,
        RedirectUri = request.RedirectUri
      }, cancellationToken);

    return result.Code;
  }

  private async Task<string> CreateGrant(SilentTokenLoginCommand request, string userId, CancellationToken cancellationToken)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(c => c.RedirectUris)
      .SingleAsync(c => c.Id == request.ClientId, cancellationToken: cancellationToken);

    var session = await _identityContext
      .Set<User>()
      .Where(u => u.Id == userId)
      .SelectMany(u => u.Sessions)
      .Where(s => !s.IsRevoked)
      .SingleAsync(cancellationToken: cancellationToken);

    var maxAge = string.IsNullOrWhiteSpace(request.MaxAge) ? client.DefaultMaxAge : long.Parse(request.MaxAge);
    var result = await _authorizationGrantService.CreateAuthorizationGrant(
      new CreateAuthorizationGrantArguments
      {
        Client = client,
        Session = session,
        CodeChallenge = request.CodeChallenge,
        CodeChallengeMethod = request.CodeChallengeMethod,
        Nonce = request.Nonce,
        Scope = request.Scope,
        RedirectUri = request.RedirectUri,
        MaxAge = maxAge
      }, cancellationToken);

    return result.Code;
  }
}