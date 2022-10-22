using Infrastructure.Builders;

namespace Infrastructure.Decoders.Abstractions;
public interface ICodeDecoder
{
  AuthorizationCode DecodeAuthorizationCode(string code);
}
