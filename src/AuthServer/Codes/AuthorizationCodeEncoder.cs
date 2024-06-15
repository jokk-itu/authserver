using System.Text;
using System.Text.Json;
using AuthServer.Codes.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Codes;
internal class AuthorizationCodeEncoder : IAuthorizationCodeEncoder
{
    private readonly IDataProtector _dataProtector;

    public AuthorizationCodeEncoder(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("AuthorizationCode");
    }

    /// <inheritdoc/>
    public string EncodeAuthorizationCode(EncodedAuthorizationCode authorizationCode)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, false);
        writer.Write(JsonSerializer.Serialize(authorizationCode));
        var protectedBytes = _dataProtector.Protect(ms.ToArray());
        return Base64UrlEncoder.Encode(protectedBytes);
    }

    /// <inheritdoc/>
    public EncodedAuthorizationCode? DecodeAuthorizationCode(string authorizationCode)
    {
        var decoded = Base64UrlEncoder.DecodeBytes(authorizationCode);
        var unProtectedBytes = _dataProtector.Unprotect(decoded);
        using var ms = new MemoryStream(unProtectedBytes);
        using var reader = new BinaryReader(ms, Encoding.UTF8, false);
        var deserializedCode = JsonSerializer.Deserialize<EncodedAuthorizationCode>(reader.ReadString());
        return deserializedCode;
    }
}