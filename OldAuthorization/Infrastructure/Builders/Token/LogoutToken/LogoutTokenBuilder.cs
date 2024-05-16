using System.IdentityModel.Tokens.Jwt;
using Application;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Builders.Token.LogoutToken;
public class LogoutTokenBuilder : ITokenBuilder<LogoutTokenArguments>
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly JwkManager _jwkManager;

  public LogoutTokenBuilder(
    IdentityConfiguration identityConfiguration,
    JwkManager jwkManager)
  {
    _identityConfiguration = identityConfiguration;
    _jwkManager = jwkManager;
  }

  public Task<string> BuildToken(LogoutTokenArguments arguments)
  {
    var claims = new Dictionary<string, object>
    {
      { ClaimNameConstants.Aud, arguments.ClientId },
      { ClaimNameConstants.Sid, arguments.SessionId },
      { ClaimNameConstants.Sub, arguments.UserId },
      { ClaimNameConstants.Jti, Guid.NewGuid() },
      { ClaimNameConstants.Events, new Dictionary<string, object>
      {
        { "http://schemas.openid.net/event/backchannel-logout", new() }
      }}
    };
    var now = DateTime.UtcNow;
    // TODO deduce encryption key and algorithm from client
    var securityKey = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
    {
      KeyId = _jwkManager.KeyId
    };
    var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      IssuedAt = now,
      Expires = now.AddMinutes(10),
      NotBefore = DateTime.UtcNow,
      Issuer = _identityConfiguration.Issuer,
      SigningCredentials = signingCredentials,
      TokenType = "logout+jwt",
      Claims = claims
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    return Task.FromResult(tokenHandler.CreateEncodedJwt(tokenDescriptor));
  }
}