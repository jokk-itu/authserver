using System.Net;
using System.Security.Cryptography.X509Certificates;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.TokenIntrospection;
public class TokenIntrospectionHandler : IRequestHandler<TokenIntrospectionQuery, TokenIntrospectionResponse>
{
  private readonly IdentityContext _identityContext;

  public TokenIntrospectionHandler(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<TokenIntrospectionResponse> Handle(TokenIntrospectionQuery request, CancellationToken cancellationToken)
  {
    var query = await _identityContext
      .Set<Token>()
      .Where(x => x.Reference == request.Token)
      .OfType<GrantToken>()
      .Select(x => new
      {
        Token = x,
        x.AuthorizationGrant.Session.User.UserName,
        Subject = x.AuthorizationGrant.Session.User.Id
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (query is null)
    {
      return new TokenIntrospectionResponse(HttpStatusCode.OK)
      {
        Active = false
      };
    }

    return new TokenIntrospectionResponse(HttpStatusCode.OK)
    {
      Active = query.Token.RevokedAt is null,
      JwtId = query.Token.Id.ToString(),
      ClientId = request.ClientId,
      ExpiresAt = new DateTimeOffset(query.Token.ExpiresAt).ToUnixTimeSeconds(),
      Issuer = query.Token.Issuer,
      Audience = query.Token.Audience.Split(' '),
      IssuedAt = new DateTimeOffset(query.Token.IssuedAt).ToUnixTimeSeconds(),
      NotBefore = new DateTimeOffset(query.Token.NotBefore).ToUnixTimeSeconds(),
      Scope = query.Token.Scope,
      Subject = query.Subject,
      TokenType = query.Token.TokenType.ToString(),
      UserName = query.UserName
    };
  }
}
