using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Decoders.Token.Abstractions;
public interface IStructuredTokenDecoder
{
  Task<JwtSecurityToken> Decode(string token, StructuredTokenDecoderArguments arguments);
}