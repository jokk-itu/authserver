using System.Net;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Requests.Abstract;
using Infrastructure.Services.Abstract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentTokenLogin;
public class SilentTokenLoginHandler : AuthorizeHandler, IRequestHandler<SilentTokenLoginCommand, SilentTokenLoginResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public SilentTokenLoginHandler(
    IdentityContext identityContext,
    IAuthorizationGrantService authorizationGrantService,
    IStructuredTokenDecoder tokenDecoder)
  : base(identityContext, authorizationGrantService)
  {
    _identityContext = identityContext;
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
      code = await UpdateGrant(request, authorizationGrant, cancellationToken);
    }

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new SilentTokenLoginResponse(HttpStatusCode.OK)
    {
      Code = code
    };
  }
}