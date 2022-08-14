using AuthorizationServer.Tokens;
using Domain;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AuthorizationServer.TokenFactories;
public class AuthorizationCodeTokenFactory
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly UserManager<IdentityUserExtended> _userManager;
  private readonly IDataProtector _protector;

  public AuthorizationCodeTokenFactory(
    IdentityConfiguration identityConfiguration,
    IDataProtectionProvider protectorProvider,
    UserManager<IdentityUserExtended> userManager)
  {
    _identityConfiguration = identityConfiguration;
    _userManager = userManager;
    _protector = protectorProvider.CreateProtector(identityConfiguration.AuthorizationCodeSecret);
  }

  public async Task<string> GenerateTokenAsync(
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
      CreatedAt = DateTimeOffset.UtcNow.Ticks,
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

  public AuthorizationCode DecodeToken(string token)
  {
    var decoded = Base64UrlEncoder.DecodeBytes(token);
    var unProtectedBytes = _protector.Unprotect(decoded);
    var ms = new MemoryStream(unProtectedBytes);
    using var reader = new BinaryReader(ms, Encoding.UTF8, false);
    var code = reader.ReadString();
    return JsonSerializer.Deserialize<AuthorizationCode>(code) 
      ?? throw new Exception("AuthorizationCode is not valid");
  }

  public async Task<bool> ValidateAsync(
   string token,
   string redirectUri,
   string clientId,
   string codeVerifier)
  {
    var code = DecodeToken(token);
    var creationTime = new DateTimeOffset(code.CreatedAt, TimeSpan.Zero);
    if (creationTime + TimeSpan.FromSeconds(_identityConfiguration.AuthorizationCodeExpiration) <
        DateTimeOffset.UtcNow)
      return false;

    if (!code.RedirectUri.Equals(redirectUri))
      return false;

    if (!code.ClientId.Equals(clientId))
      return false;

    using var sha256 = SHA256.Create();
    var hashed = sha256.ComputeHash(Encoding.Default.GetBytes(codeVerifier));
    var encoded = Base64UrlEncoder.Encode(hashed);
    if (!code.CodeChallenge.Equals(encoded))
      return false;

    var user = await _userManager.FindByIdAsync(code.UserId);
    if (user is null)
      return false;

    return true;
  }
}