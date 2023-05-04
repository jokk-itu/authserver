using System.Net;
using Domain;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.TokenRevocation;
public class TokenRevocationHandler : IRequestHandler<TokenRevocationCommand, TokenRevocationResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public TokenRevocationHandler(IdentityContext identityContext, IStructuredTokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<TokenRevocationResponse> Handle(TokenRevocationCommand request, CancellationToken cancellationToken)
  {
    var token = await GetToken(request, cancellationToken);
    if (token is null)
    {
      return new TokenRevocationResponse(HttpStatusCode.OK);
    }

    token.RevokedAt = DateTime.UtcNow;
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new TokenRevocationResponse(HttpStatusCode.OK);
  }

  private async Task<Token?> GetToken(TokenRevocationCommand command, CancellationToken cancellationToken)
  {
    if (!TokenHelper.IsStructuredToken(command.Token))
    {
      return await _identityContext
        .Set<Token>()
        .Where(x => x.RevokedAt == null)
        .SingleOrDefaultAsync(x => x.Reference == command.Token, cancellationToken: cancellationToken);
    }

    var securityToken = await _tokenDecoder.Decode(command.Token, new StructuredTokenDecoderArguments
      {
        ClientId = command.ClientId,
        ValidateAudience = false,
        ValidateLifetime = false
      });
    
    var id = Guid.Parse(securityToken.Id);
    
    return await _identityContext
      .Set<Token>()
      .Where(x => x.RevokedAt == null)
      .SingleOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
  }
}