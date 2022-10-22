using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Infrastructure.Decoders.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Decoders;
public class TokenDecoder : ITokenDecoder
{
    private readonly ILogger<TokenDecoder> _logger;
    private readonly IOptions<JwtBearerOptions> _jwtBearerOptions;
    private readonly JwkManager _jwkManager;

    public TokenDecoder(
      ILogger<TokenDecoder> logger,
      IOptions<JwtBearerOptions> jwtBearerOptions,
      JwkManager jwkManager)
    {
        _logger = logger;
        _jwtBearerOptions = jwtBearerOptions;
        _jwkManager = jwkManager;
    }

    public JwtSecurityToken? DecodeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

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
}