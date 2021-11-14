using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using OAuthService.Tokens;

namespace OAuthService.TokenFactories;

public class AuthorizationCodeTokenFactory : ITokenFactory
{
    private readonly AuthenticationConfiguration _configuration;
    private readonly IDataProtector _protector;

    public AuthorizationCodeTokenFactory(AuthenticationConfiguration configuration, IDataProtector protector)
    {
        _configuration = configuration;
        _protector = protector;
    }

    public async Task<string> GenerateTokenAsync(
        [Required(AllowEmptyStrings = false)] string redirectUri, 
        [Required] ICollection<string> scopes,
        [Required(AllowEmptyStrings = false)] string clientId)
    {
        var ms = new MemoryStream();
        await using var writer = new BinaryWriter(ms, Encoding.UTF8, false);
        writer.Write(DateTimeOffset.UtcNow.ToString());
        writer.Write(redirectUri);
        writer.Write(JsonSerializer.Serialize(scopes));
        writer.Write(clientId);
        writer.Write("authorization_code");
        var protectedBytes = _protector.Protect(ms.ToArray());
        return Convert.ToBase64String(protectedBytes);
    }

    public async Task<AuthorizationCode> DecodeTokenAsync(string token)
    {
        try
        {
            var decoded = Convert.FromBase64String(token);
            var unProtectedBytes = _protector.Unprotect(decoded);
            var ms = new MemoryStream(unProtectedBytes);
            using var reader = new BinaryReader(ms, Encoding.UTF8, false);
            var time = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
            var redirectUri = reader.ReadString();
            var scopes = JsonSerializer.Deserialize<ICollection<string>>(reader.ReadString());
            var clientId = reader.ReadString();
            var purpose = reader.ReadString();
            return new AuthorizationCode
            {
                RedirectUri = redirectUri,
                Scopes = scopes,
                ClientId = clientId
            };
        }
        catch (Exception ex) when (ex is EndOfStreamException or IOException or ObjectDisposedException or FormatException)
        {
            throw;
        }
    }
    
    public async Task<bool> ValidateAsync(
        [Required(AllowEmptyStrings = false)] string purpose,
        [Required(AllowEmptyStrings = false)] string token,
        [Required(AllowEmptyStrings = false)] string redirectUri,
        [Required(AllowEmptyStrings = false)] string clientId)
    {
        try
        {
            var decoded = Convert.FromBase64String(token);
            var unProtectedBytes = _protector.Unprotect(decoded);
            var ms = new MemoryStream(unProtectedBytes);
            using var reader = new BinaryReader(ms, Encoding.UTF8, false);
            var creationTime = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
            if (creationTime + TimeSpan.FromSeconds(_configuration.AuthorizationCodeTokenExpiration) <
                DateTimeOffset.UtcNow)
                return false;

            var creationRedirectUri = reader.ReadString();
            if (!creationRedirectUri.Equals(redirectUri))
                return false;

            var creationScopes = reader.ReadString();

            var creationClientId = reader.ReadString();
            if (!creationClientId.Equals(clientId))
                return false;

            var creationPurpose = reader.ReadString();
            if (!creationPurpose.Equals(purpose))
                return false;

            return true;
        }
        catch (Exception ex) when (ex is EndOfStreamException or IOException or ObjectDisposedException or FormatException)
        {
            throw;
        }
    } 
}