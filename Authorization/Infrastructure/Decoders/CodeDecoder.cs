using System.Text;
using System.Text.Json;
using Infrastructure.Builders;
using Infrastructure.Decoders.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Decoders;
public class CodeDecoder : ICodeDecoder
{
  private readonly IDataProtector _dataProtector;

  public CodeDecoder(IDataProtectionProvider dataProtectionProvider)
  {
    _dataProtector = dataProtectionProvider.CreateProtector("AuthorizationCode");
  }

  public AuthorizationCode DecodeAuthorizationCode(string code)
  {
    var decoded = Base64UrlEncoder.DecodeBytes(code);
    var unProtectedBytes = _dataProtector.Unprotect(decoded);
    var ms = new MemoryStream(unProtectedBytes);
    using var reader = new BinaryReader(ms, Encoding.UTF8, false);
    var deserializedCode = JsonSerializer.Deserialize<AuthorizationCode>(reader.ReadString());
    return deserializedCode ?? throw new InvalidOperationException();
  }
}