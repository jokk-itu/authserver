using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Factories.TokenFactories.Abstractions;
public abstract class TokenFactory
{
  protected readonly IdentityConfiguration _identityConfiguration;
  protected readonly TokenValidationParameters _tokenValidationParameters;
  protected readonly JwkManager _jwkManager;
  protected readonly ILogger<TokenFactory> _logger;

  protected TokenFactory(
    ILogger<TokenFactory> logger,
    IdentityConfiguration identityConfiguration,
    TokenValidationParameters tokenValidationParameters,
    JwkManager jwkManager
    )
  {
    _jwkManager = jwkManager;
    _identityConfiguration = identityConfiguration;
    _tokenValidationParameters = tokenValidationParameters;
    _logger = logger;
  }

  protected string GetSignedToken(
    IEnumerable<Claim> claims,
    string audience,
    DateTime expires)
  {
    var key = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
    {
      KeyId = _jwkManager.KeyId
    };

    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    var securityToken = new JwtSecurityToken(
        issuer: _identityConfiguration.InternalIssuer,
        audience: audience,
        notBefore: DateTime.Now,
        expires: expires,
        claims: claims,
        signingCredentials: signingCredentials);

    var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
    return token;
  }

  public JwtSecurityToken DecodeToken(string token)
  {
    new JwtSecurityTokenHandler()
        .ValidateToken(token, _tokenValidationParameters, out var validatedToken);
    return (JwtSecurityToken)validatedToken;
  }

  public bool ValidateToken(string token)
  {
    new JwtSecurityTokenHandler()
        .ValidateToken(token, _tokenValidationParameters, out _);
    return true;
  }
}
