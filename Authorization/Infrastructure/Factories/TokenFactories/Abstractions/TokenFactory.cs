using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Factories.TokenFactories.Abstractions;
public abstract class TokenFactory
{
  protected readonly IdentityConfiguration _identityConfiguration;
  protected readonly JwtBearerOptions _jwtBearerOptions;
  protected readonly JwkManager _jwkManager;
  protected readonly ILogger<TokenFactory> _logger;

  protected TokenFactory(
    ILogger<TokenFactory> logger,
    IdentityConfiguration identityConfiguration,
    JwtBearerOptions jwtBearerOptions,
    JwkManager jwkManager
    )
  {
    _jwkManager = jwkManager;
    _identityConfiguration = identityConfiguration;
    _jwtBearerOptions = jwtBearerOptions;
    _logger = logger;
  }

  protected string GetSignedToken(
    IDictionary<string, object> claims,
    DateTime expires)
  {
    var key = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
    {
      KeyId = _jwkManager.KeyId
    };
    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    var tokenDescriptor = new SecurityTokenDescriptor 
    {
      IssuedAt = DateTime.Now,
      Expires = expires,
      NotBefore = DateTime.Now,
      Issuer = _identityConfiguration.InternalIssuer,
      SigningCredentials = signingCredentials,
      Claims = claims
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }

  public async Task<JwtSecurityToken?> DecodeTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(token))
      return null;

    var configuration = await _jwtBearerOptions.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
    var tokenValidationParameters = new TokenValidationParameters 
    {
      IssuerSigningKeys = configuration.SigningKeys,
      ValidIssuer = _jwtBearerOptions.Authority,
      ValidAudience = _jwtBearerOptions.Audience,
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateIssuerSigningKey = true
    };

    try
    {
      new JwtSecurityTokenHandler()
        .ValidateToken(token, tokenValidationParameters, out var validatedToken);
      return validatedToken as JwtSecurityToken;
    }
    catch(SecurityTokenException exception)
    {
      _logger.LogError(exception, "Token {token} is invalid", token);
      return null;
    }
  }

  public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(token))
      return false;

    var configuration = await _jwtBearerOptions.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
    var tokenValidationParameters = new TokenValidationParameters
    {
      IssuerSigningKeys = configuration.SigningKeys,
      ValidIssuer = _jwtBearerOptions.Authority,
      ValidAudience = _jwtBearerOptions.Audience,
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateIssuerSigningKey = true
    };

    try
    {
      new JwtSecurityTokenHandler()
        .ValidateToken(token, tokenValidationParameters, out var _);
      return true;
    }
    catch (SecurityTokenException exception)
    {
      _logger.LogError(exception, "Token {token} is invalid", token);
      return false;
    }
  }
}