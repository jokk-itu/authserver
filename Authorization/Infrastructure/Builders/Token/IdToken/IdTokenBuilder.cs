using System.IdentityModel.Tokens.Jwt;
using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Builders.Token.IdToken;
public class IdTokenBuilder : ITokenBuilder<IdTokenArguments>
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly IdentityContext _identityContext;
  private readonly JwkManager _jwkManager;

  public IdTokenBuilder(
    IdentityConfiguration identityConfiguration,
    IdentityContext identityContext,
    JwkManager jwkManager)
  {
    _identityConfiguration = identityConfiguration;
    _identityContext = identityContext;
    _jwkManager = jwkManager;
  }

  public Task<JwtSecurityToken> GetToken(string token)
  {
    throw new NotImplementedException();
  }

  public async Task<string> BuildToken(IdTokenArguments arguments)
  {
    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == arguments.AuthorizationGrantId)
      .Include(x => x.Nonces)
      .Select(x => new
      {
        ClientId = x.Client.Id,
        SessionId = x.Session.Id,
        UserId = x.Session.User.Id,
        Nonce = x.Nonces.OrderByDescending(y => y.IssuedAt).First()
      })
      .SingleAsync();

    var claims = new Dictionary<string, object>
    {
      { ClaimNameConstants.Aud, query.ClientId },
      { ClaimNameConstants.Sid, query.SessionId },
      { ClaimNameConstants.Sub, query.UserId },
      { ClaimNameConstants.Jti, Guid.NewGuid() },
      { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId },
      { ClaimNameConstants.Nonce, query.Nonce.Value }
    };
    var now = DateTime.UtcNow;
    // TODO deduce encryption and signing from IdToken
    var securityKey = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
    {
      KeyId = _jwkManager.KeyId
    };
    var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      IssuedAt = now,
      Expires = now.AddHours(1),
      NotBefore = DateTime.UtcNow,
      Issuer = _identityConfiguration.Issuer,
      SigningCredentials = signingCredentials,
      TokenType = "id+jwt",
      Claims = claims
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    return tokenHandler.CreateEncodedJwt(tokenDescriptor);
  }
}
