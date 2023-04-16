using System.Net;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.TokenRevocation;
public class TokenRevocationHandler : IRequestHandler<TokenRevocationCommand, TokenRevocationResponse>
{
  private readonly IdentityContext _identityContext;

  public TokenRevocationHandler(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<TokenRevocationResponse> Handle(TokenRevocationCommand request, CancellationToken cancellationToken)
  {
    var token = await _identityContext
      .Set<Token>()
      .Where(x => x.Reference == request.Token)
      .Where(x => x.RevokedAt == null)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (token is null)
    {
      return new TokenRevocationResponse(HttpStatusCode.OK);
    }

    token.RevokedAt = DateTime.UtcNow;
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new TokenRevocationResponse(HttpStatusCode.OK);
  }
}