using AuthorizationServer.Repositories;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthorizationServer.TokenFactories;

public class AccessTokenFactory
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly TokenValidationParameters _tokenValidationParameters;
  private readonly ResourceManager _resourceManager;
  private readonly JwkManager _jwkManager;

  public AccessTokenFactory(
      IdentityConfiguration identityConfiguration,
      TokenValidationParameters tokenValidationParameters,
      ResourceManager resourceManager,
      JwkManager jwkManager)
  {
    _identityConfiguration = identityConfiguration;
    _tokenValidationParameters = tokenValidationParameters;
    _resourceManager = resourceManager;
    _jwkManager = jwkManager;
  }

  public async Task<string> GenerateTokenAsync(string clientId, ICollection<string> scopes,
      string userId)
  {
    var exp = DateTime.Now + TimeSpan.FromSeconds(_identityConfiguration.AccessTokenExpiration);
    var resources = await _resourceManager.FindResourcesByScopes(scopes);
    var aud = resources.Aggregate(string.Empty, (acc, r) => $"{acc} {r.Id}");
    aud = aud.Trim();
    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, userId),
      new Claim("scope", scopes.Aggregate((elem, acc) => $"{acc} {elem}")),
      new Claim("client_id", clientId)
    };

    var key = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
    {
      KeyId = _jwkManager.KeyId
    };
    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    var securityToken = new JwtSecurityToken(
        issuer: _identityConfiguration.InternalIssuer,
        audience: aud,
        notBefore: DateTime.Now,
        expires: exp,
        claims: claims,
        signingCredentials: signingCredentials);

    var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
    return await Task.FromResult(token);
  }

  public Task<JwtSecurityToken> DecodeTokenAsync(string token)
  {
    new JwtSecurityTokenHandler()
        .ValidateToken(token, _tokenValidationParameters, out var validatedToken);
    return Task.FromResult((JwtSecurityToken)validatedToken);
  }

  public async Task<bool> ValidateTokenAsync(string token)
  {
    new JwtSecurityTokenHandler()
        .ValidateToken(token, _tokenValidationParameters, out _);
    return await Task.FromResult(true);
  }
}