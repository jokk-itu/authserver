using System.IdentityModel.Tokens.Jwt;
using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Builders.Token.GrantAccessToken;

public class GrantAccessTokenBuilder : ITokenBuilder<GrantAccessTokenArguments>
{
  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly JwkManager _jwkManager;

  public GrantAccessTokenBuilder(
    IdentityContext identityContext,
    IdentityConfiguration identityConfiguration,
    JwkManager jwkManager)
  {
    _identityContext = identityContext;
    _identityConfiguration = identityConfiguration;
    _jwkManager = jwkManager;
  }

  public async Task<string> BuildToken(GrantAccessTokenArguments arguments)
  {
    if (_identityConfiguration.UseReferenceTokens)
    {
      return await BuildReferenceToken(arguments);
    }

    return await BuildStructuredToken(arguments);
  }

  private async Task<string> BuildStructuredToken(GrantAccessTokenArguments arguments)
  {
    var claims = new Dictionary<string, object>
    {
      { ClaimNameConstants.Jti, Guid.NewGuid() },
      { ClaimNameConstants.Scope, arguments.Scope },
      { ClaimNameConstants.Aud, arguments.ResourceNames },
      { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId }
    };

    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == arguments.AuthorizationGrantId)
      .Select(x => new
      {
        SessionId = x.Session.Id,
        UserId = x.Session.User.Id
      })
      .SingleAsync();

    claims.Add(ClaimNameConstants.Sub, query.UserId);
    claims.Add(ClaimNameConstants.Sid, query.SessionId);

    var now = DateTime.UtcNow;
    var key = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
    {
      KeyId = _jwkManager.KeyId
    };
    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      IssuedAt = now,
      Expires = now.AddHours(1),
      NotBefore = now,
      Issuer = _identityConfiguration.Issuer,
      SigningCredentials = signingCredentials,
      TokenType = "access+jwt",
      Claims = claims
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    return tokenHandler.CreateEncodedJwt(tokenDescriptor);
  }

  private async Task<string> BuildReferenceToken(GrantAccessTokenArguments arguments)
  {
    var grant = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == arguments.AuthorizationGrantId)
      .SingleAsync();

    var now = DateTime.UtcNow;

    var accessToken = new Domain.GrantAccessToken
    {
      AuthorizationGrant = grant,
      Audience = string.Join(" ", arguments.ResourceNames),
      IssuedAt = now,
      NotBefore = now,
      ExpiresAt = now.AddHours(1),
      Scope = arguments.Scope,
      Issuer = _identityConfiguration.Issuer
    };

    await _identityContext.Set<Domain.GrantAccessToken>().AddAsync(accessToken);
    return accessToken.Reference;
  }
}