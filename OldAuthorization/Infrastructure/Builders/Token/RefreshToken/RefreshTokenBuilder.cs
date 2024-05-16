using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Builders.Token.RefreshToken;
public class RefreshTokenBuilder : ITokenBuilder<RefreshTokenArguments>
{
  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly JwkManager _jwkManager;

  public RefreshTokenBuilder(
    IdentityContext identityContext,
    IdentityConfiguration identityConfiguration,
    JwkManager jwkManager)
  {
    _identityContext = identityContext;
    _identityConfiguration = identityConfiguration;
    _jwkManager = jwkManager;
  }

  public async Task<string> BuildToken(RefreshTokenArguments arguments)
  {
    if (_identityConfiguration.UseReferenceTokens)
    {
      return await BuildReferenceToken(arguments);
    }

    return await BuildStructuredToken(arguments);
  }

  private async Task<string> BuildReferenceToken(RefreshTokenArguments arguments)
  {
    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == arguments.AuthorizationGrantId)
      .Select(x => new
      {
        ClientId = x.Client.Id,
        AuthorizationGrant = x,
      })
      .SingleAsync();

    var now = DateTime.UtcNow;
    var refreshToken = new Domain.RefreshToken
    {
      AuthorizationGrant = query.AuthorizationGrant,
      Audience = query.ClientId,
      Issuer = _identityConfiguration.Issuer,
      ExpiresAt = now.AddDays(30),
      IssuedAt = now,
      NotBefore = now,
      Scope = arguments.Scope
    };
    await _identityContext
      .Set<Domain.RefreshToken>()
      .AddAsync(refreshToken);

    return refreshToken.Reference;
  }

  private async Task<string> BuildStructuredToken(RefreshTokenArguments arguments)
  {
    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == arguments.AuthorizationGrantId)
      .Select(x => new
      {
        AuthorizationGrant = x,
        ClientId = x.Client.Id,
        SessionId = x.Session.Id,
        UserId = x.Session.User.Id,
      })
      .SingleAsync();

    var now = DateTime.UtcNow;
    var refreshToken = new Domain.RefreshToken
    {
      AuthorizationGrant = query.AuthorizationGrant,
      Audience = query.ClientId,
      Issuer = _identityConfiguration.Issuer,
      ExpiresAt = now.AddDays(30),
      IssuedAt = now,
      NotBefore = now,
      Scope = arguments.Scope
    };

    await _identityContext.Set<Domain.RefreshToken>().AddAsync(refreshToken);

    var claims = new Dictionary<string, object>
    {
      { ClaimNameConstants.Aud, query.ClientId },
      { ClaimNameConstants.Sid, query.SessionId },
      { ClaimNameConstants.Sub, query.UserId },
      { ClaimNameConstants.Jti, refreshToken.Id },
      { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId }
    };
    
    var securityKey = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
    {
      KeyId = _jwkManager.KeyId
    };
    var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      IssuedAt = now,
      Expires = now.AddDays(30),
      NotBefore = DateTime.UtcNow,
      Issuer = _identityConfiguration.Issuer,
      SigningCredentials = signingCredentials,
      TokenType = "refresh+jwt",
      Claims = claims
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    return tokenHandler.CreateEncodedJwt(tokenDescriptor);
  }
}
