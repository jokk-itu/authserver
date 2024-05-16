using Application;
using Domain.Constants;
using Domain;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Builders.Token.ClientAccessToken;
public class ClientAccessTokenBuilder : ITokenBuilder<ClientAccessTokenArguments>
{
  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly JwkManager _jwkManager;

  public ClientAccessTokenBuilder(
    IdentityContext identityContext,
    IdentityConfiguration identityConfiguration,
    JwkManager jwkManager)
  {
    _identityContext = identityContext;
    _identityConfiguration = identityConfiguration;
    _jwkManager = jwkManager;
  }

  public async Task<string> BuildToken(ClientAccessTokenArguments arguments)
  {
    if (_identityConfiguration.UseReferenceTokens)
    {
      return await BuildReferenceToken(arguments);
    }

    return BuildStructuredToken(arguments);
  }

  private string BuildStructuredToken(ClientAccessTokenArguments arguments)
  {
    var claims = new Dictionary<string, object>
    {
      { ClaimNameConstants.Jti, Guid.NewGuid() },
      { ClaimNameConstants.Scope, arguments.Scope },
      { ClaimNameConstants.Aud, arguments.Resource }
    };

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

  private async Task<string> BuildReferenceToken(ClientAccessTokenArguments arguments)
  {
    var client = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == arguments.ClientId)
      .SingleAsync();

    var now = DateTime.UtcNow;

    var accessToken = new Domain.ClientAccessToken()
    {
      Client = client,
      Audience = string.Join(" ", arguments.Resource),
      IssuedAt = now,
      NotBefore = now,
      ExpiresAt = now.AddHours(1),
      Scope = arguments.Scope,
      Issuer = _identityConfiguration.Issuer
    };

    await _identityContext.Set<Domain.ClientAccessToken>().AddAsync(accessToken);
    return accessToken.Reference;
  }
}
