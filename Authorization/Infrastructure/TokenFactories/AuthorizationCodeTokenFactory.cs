using AuthorizationServer.Tokens;
using Microsoft.AspNetCore.DataProtection;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AuthorizationServer.TokenFactories;

public class AuthorizationCodeTokenFactory
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly IDataProtector _protector;

  public AuthorizationCodeTokenFactory(
    IdentityConfiguration identityConfiguration,
    IDataProtectionProvider protectorProvider)
  {
    _identityConfiguration = identityConfiguration;
    _protector = protectorProvider.CreateProtector(identityConfiguration.AuthorizationCodeSecret);
  }

  public async Task<string> GenerateTokenAsync(
      [Required] string redirectUri,
      [Required] ICollection<string> scopes,
      [Required] string clientId,
      [Required] string codeChallenge,
      [Required] string codeChallengeMethod,
      [Required] string userId)
  {
    var ms = new MemoryStream();
    await using var writer = new BinaryWriter(ms, Encoding.UTF8, false);
    writer.Write(DateTimeOffset.UtcNow.UtcTicks);
    writer.Write(redirectUri);
    writer.Write(JsonSerializer.Serialize(scopes));
    writer.Write(clientId);
    writer.Write(codeChallenge);
    writer.Write(codeChallengeMethod);
    writer.Write(userId);
    writer.Write("authorization_code");
    var protectedBytes = _protector.Protect(ms.ToArray());
    return Convert.ToBase64String(protectedBytes);
  }

  public Task<AuthorizationCode> DecodeTokenAsync(string token)
  {
    var decoded = Convert.FromBase64String(token);
    var unProtectedBytes = _protector.Unprotect(decoded);
    var ms = new MemoryStream(unProtectedBytes);
    using var reader = new BinaryReader(ms, Encoding.UTF8, false);
    var time = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
    var redirectUri = reader.ReadString();
    var scopes = JsonSerializer.Deserialize<ICollection<string>>(reader.ReadString());
    var clientId = reader.ReadString();
    var codeChallenge = reader.ReadString();
    var codeChallengeMethod = reader.ReadString();
    var userId = reader.ReadString();
    var purpose = reader.ReadString();
    return Task.FromResult(new AuthorizationCode
    {
      RedirectUri = redirectUri,
      Scopes = scopes,
      ClientId = clientId,
      CodeChallenge = codeChallenge,
      UserId = userId
    });
  }

  public Task<bool> ValidateAsync(
      [Required] string purpose,
      [Required] string token,
      [Required] string redirectUri,
      [Required] string clientId,
      [Required] string codeVerifier)
  {
    var decoded = Convert.FromBase64String(token);
    var unProtectedBytes = _protector.Unprotect(decoded);
    var ms = new MemoryStream(unProtectedBytes);
    using var reader = new BinaryReader(ms, Encoding.Default, false);
    var creationTime = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
    if (creationTime + TimeSpan.FromSeconds(_identityConfiguration.AuthorizationCodeExpiration) <
        DateTimeOffset.UtcNow)
      return Task.FromResult(false);

    var creationRedirectUri = reader.ReadString();
    if (!creationRedirectUri.Equals(redirectUri))
      return Task.FromResult(false);

    var creationScopes = reader.ReadString();

    var creationClientId = reader.ReadString();
    if (!creationClientId.Equals(clientId))
      return Task.FromResult(false);

    var codeChallenge = reader.ReadString();
    var codeChallengeMethod = reader.ReadString();
    using var sha256 = SHA256.Create();
    var hashed = sha256.ComputeHash(Encoding.Default.GetBytes(codeVerifier));
    var encoded = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(hashed);
    if (!codeChallenge.Equals(encoded))
      return Task.FromResult(false);

    var userId = reader.ReadString();
    var creationPurpose = reader.ReadString();
    if (!creationPurpose.Equals(purpose))
      return Task.FromResult(false);

    return Task.FromResult(true);
  }
}