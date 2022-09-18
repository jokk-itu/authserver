using Domain;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Factories.CodeFactories;
public class CodeFactory
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly UserManager<User> _userManager;
    private readonly IDataProtector _protector;

    public CodeFactory(
      IdentityConfiguration identityConfiguration,
      IDataProtectionProvider protectorProvider,
      UserManager<User> userManager)
    {
        _identityConfiguration = identityConfiguration;
        _userManager = userManager;
        _protector = protectorProvider.CreateProtector(identityConfiguration.CodeSecret);
    }

    public async Task<string> GenerateCodeAsync(
      string redirectUri,
      ICollection<string> scopes,
      string clientId,
      string codeChallenge,
      string userId,
      string nonce)
    {
        var ms = new MemoryStream();
        await using var writer = new BinaryWriter(ms, Encoding.UTF8, false);
        var code = new AuthorizationCode
        {
            CreatedAt = DateTime.UtcNow.Ticks,
            ClientId = clientId,
            RedirectUri = redirectUri,
            Scopes = scopes,
            CodeChallenge = codeChallenge,
            UserId = userId,
            Nonce = nonce
        };
        writer.Write(JsonSerializer.Serialize(code));
        var protectedBytes = _protector.Protect(ms.ToArray());
        return Base64UrlEncoder.Encode(protectedBytes);
    }

    public AuthorizationCode? DecodeCode(string token)
    {
        var decoded = Base64UrlEncoder.DecodeBytes(token);
        var unProtectedBytes = _protector.Unprotect(decoded);
        var ms = new MemoryStream(unProtectedBytes);
        using var reader = new BinaryReader(ms, Encoding.UTF8, false);
        var code = reader.ReadString();
        return JsonSerializer.Deserialize<AuthorizationCode>(code);
    }

    public async Task<bool> ValidateAsync(
     string code,
     string redirectUri,
     string clientId,
     string codeVerifier)
    {
        var decodedCode = DecodeCode(code);
        if (decodedCode is null)
            return false;

        if (new DateTime(decodedCode.CreatedAt) + TimeSpan.FromSeconds(_identityConfiguration.CodeExpiration) <
            DateTime.UtcNow)
            return false;

        if (!decodedCode.RedirectUri.Equals(redirectUri))
            return false;

        if (!decodedCode.ClientId.Equals(clientId))
            return false;

        using var sha256 = SHA256.Create();
        var hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        var encoded = Base64UrlEncoder.Encode(hashed);
        if (!decodedCode.CodeChallenge.Equals(encoded))
            return false;

        var user = await _userManager.FindByIdAsync(decodedCode.UserId);
        if (user is null)
            return false;

        return true;
    }
}