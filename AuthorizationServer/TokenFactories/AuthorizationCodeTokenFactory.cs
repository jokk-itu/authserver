using Microsoft.AspNetCore.DataProtection;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using AuthorizationServer.Tokens;

namespace AuthorizationServer.TokenFactories;

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
      [Required(AllowEmptyStrings = false)] string clientId,
      [Required(AllowEmptyStrings = false)] string codeChallenge,
      [Required(AllowEmptyStrings = false)] string codeChallengeMethod,
      [Required(AllowEmptyStrings = false)] string userId)
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
      [Required(AllowEmptyStrings = false)] string purpose,
      [Required(AllowEmptyStrings = false)] string token,
      [Required(AllowEmptyStrings = false)] string redirectUri,
      [Required(AllowEmptyStrings = false)] string clientId,
      [Required(AllowEmptyStrings = false)] string codeVerifier)
  {
    var decoded = Convert.FromBase64String(token);
    var unProtectedBytes = _protector.Unprotect(decoded);
    var ms = new MemoryStream(unProtectedBytes);
    using var reader = new BinaryReader(ms, Encoding.UTF8, false);
    var creationTime = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
    if (creationTime + TimeSpan.FromSeconds(_configuration.AuthorizationCodeExpiration) <
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
    switch (codeChallengeMethod)
    {
      case "plain":
        if (!codeChallenge.Equals(codeVerifier))
          return Task.FromResult(false);
        break;
      case "S256":
        var encoded = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(codeVerifier));
        var hashed = encoded.Sha256();
        var base64 = hashed.Base64Encode();
        if (!codeChallenge.Equals(base64))
          return Task.FromResult(false);
        break;
    }

    var userId = reader.ReadString();
    //TODO validate UserId

    var creationPurpose = reader.ReadString();
    if (!creationPurpose.Equals(purpose))
      return Task.FromResult(false);

    return Task.FromResult(true);
  }
}