using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application;
using Infrastructure.Decoders.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Decoders;
public class TokenDecoder : ITokenDecoder
{
    private readonly ILogger<TokenDecoder> _logger;
    private readonly IOptions<JwtBearerOptions> _jwtBearerOptions;
    private readonly JwkManager _jwkManager;
    private readonly IdentityConfiguration _identityConfiguration;

    public TokenDecoder(
      ILogger<TokenDecoder> logger,
      IOptions<JwtBearerOptions> jwtBearerOptions,
      JwkManager jwkManager,
      IdentityConfiguration identityConfiguration)
    {
        _logger = logger;
        _jwtBearerOptions = jwtBearerOptions;
        _jwkManager = jwkManager;
        _identityConfiguration = identityConfiguration;
    }

    public JwtSecurityToken? DecodeSignedToken(string token)
    {
      if (string.IsNullOrWhiteSpace(token))
      {
        return null;
      }

      var signingKeys = _jwkManager.Jwks
          .Select(x => new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider) { KeyId = x.KeyId.ToString() })
          .ToList();

        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = signingKeys,
            ValidIssuer = _jwtBearerOptions.Value.Authority,
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true
        };

        try
        {
            new JwtSecurityTokenHandler()
              .ValidateToken(token, tokenValidationParameters, out var validatedToken);
            return validatedToken as JwtSecurityToken;
        }
        catch (SecurityTokenException exception)
        {
            _logger.LogError(exception, "Token {token} is invalid", token);
            return null;
        }
    }

    public JwtSecurityToken? DecodeEncryptedToken(string token)
    {
      if (string.IsNullOrWhiteSpace(token))
        return null;

      var signingKeys = _jwkManager.Jwks
        .Select(x => new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider) { KeyId = x.KeyId.ToString() })
        .ToList();
      var encryptingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_identityConfiguration.EncryptingKeySecret));

      var tokenValidationParameters = new TokenValidationParameters
      {
        IssuerSigningKeys = signingKeys,
        TokenDecryptionKey = encryptingKey,
        ValidIssuer = _jwtBearerOptions.Value.Authority,
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true
      };

      try
      {
        new JwtSecurityTokenHandler()
          .ValidateToken(token, tokenValidationParameters, out var validatedToken);
        return validatedToken as JwtSecurityToken;
      }
      catch (SecurityTokenException exception)
      {
        _logger.LogError(exception, "Token {token} is invalid", token);
        return null;
      }
    }
}