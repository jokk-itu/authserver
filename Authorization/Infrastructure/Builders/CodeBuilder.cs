using System.Text;
using System.Text.Json;
using Application;
using Infrastructure.Builders.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Builders;
public class CodeBuilder : ICodeBuilder
{
  private readonly IDataProtector _dataProtector;

  public CodeBuilder(
    IdentityConfiguration identityConfiguration,
    IDataProtectionProvider protectorProvider)
  {
    _dataProtector = protectorProvider.CreateProtector(identityConfiguration.CodeSecret);
  }

  public async Task<string> BuildAuthorizationCodeAsync(
    string authorizationGrantId,
    string authorizationCodeId,
    string nonceId,
    string codeChallenge, 
    string codeChallengeMethod,
    ICollection<string> scopes,
    string? redirectUri = null)
  {
    using var ms = new MemoryStream();
    await using var writer = new BinaryWriter(ms, Encoding.UTF8, false);
    var authorizationCode = new AuthorizationCode
    {
      AuthorizationGrantId = authorizationGrantId,
      AuthorizationCodeId = authorizationCodeId,
      NonceId = nonceId,
      CodeChallenge = codeChallenge,
      CodeChallengeMethod = codeChallengeMethod,
      Scopes = scopes,
      RedirectUri = redirectUri
    };
    writer.Write(JsonSerializer.Serialize(authorizationCode));
    var protectedBytes = _dataProtector.Protect(ms.ToArray());
    return Base64UrlEncoder.Encode(protectedBytes);
  }
}

public class AuthorizationCode
{
  public string AuthorizationGrantId { get; init; } = null!;
  public string AuthorizationCodeId { get; init; } = null!;
  public string NonceId { get; init; } = null!;
  public string CodeChallenge { get; init; } = null!;
  public string CodeChallengeMethod { get; init; } = null!;
  public string? RedirectUri { get; init; }
  public ICollection<string> Scopes { get; init; } = new List<string>();
}