using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Decoders.Abstractions;
public interface ITokenDecoder
{
  JwtSecurityToken? DecodeToken(string token);
}
