using System.IdentityModel.Tokens.Jwt;
using Application;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Decoders.Token;
#nullable disable
public class StructuredTokenDecoder : IStructuredTokenDecoder
{
  private readonly IdentityConfiguration _identityConfiguration; 
  private readonly JwkManager _jwkManager;
  private readonly ILogger<StructuredTokenDecoder> _logger;

  public StructuredTokenDecoder(
    IdentityConfiguration identityConfiguration,
    JwkManager jwkManager,
    ILogger<StructuredTokenDecoder> logger)
  {
    _identityConfiguration = identityConfiguration;
    _jwkManager = jwkManager;
    _logger = logger;
  }

  public async Task<JwtSecurityToken> Decode(string token, StructuredTokenDecoderArguments arguments)
  {
    ValidateTokenStructure(token);

    if (token.Split('.').Length == 3)
    {
      return DecodeSignedToken(token, arguments);
    }

    return await DecodeEncryptedToken(token, arguments);
  }

  private void ValidateTokenStructure(string token)
  {
    var t = token?.Split('.').Length;
    if (t != 3 && t != 5)
    {
      throw new ArgumentException("Must be a signed or encrypted jwt", nameof(token));
    }
  }

  private JwtSecurityToken DecodeSignedToken(string token, StructuredTokenDecoderArguments arguments)
  {
    var signingKeys = _jwkManager.Jwks
      .Select(x => new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
      {
        KeyId = x.Id.ToString()
      })
      .ToList();

    var tokenValidationParameters = new TokenValidationParameters
    {
      ClockSkew = new TimeSpan(0),
      ValidTypes = new []
      {
        "access+jwt", "id+jwt", "refresh+jwt"
      },
      ValidIssuer = _identityConfiguration.Issuer,
      ValidAudiences = arguments.Audiences,
      IssuerSigningKeys = signingKeys,
      ValidateLifetime = arguments.ValidateLifetime,
      ValidateAudience = arguments.ValidateAudience,
    };

    try
    {
      new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out var validatedToken);
      return validatedToken as JwtSecurityToken;
    }
    catch (Exception e)
    {
      _logger.LogError(e, "SignedToken validation failed");
      throw;
    }
  }

  private async Task<JwtSecurityToken> DecodeEncryptedToken(string token, StructuredTokenDecoderArguments arguments)
  {
    throw new NotImplementedException();
  }
}