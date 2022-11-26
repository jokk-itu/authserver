using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Decoders.Abstractions;
public interface ITokenDecoder
{
  JwtSecurityToken? DecodeSignedToken(string token);
  JwtSecurityToken? DecodeEncryptedToken(string token);
}
