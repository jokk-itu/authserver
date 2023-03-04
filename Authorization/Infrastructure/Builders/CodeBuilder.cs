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
    ICollection<string> scopes)
  {
    var ms = new MemoryStream();
    await using var writer = new BinaryWriter(ms, Encoding.UTF8, false);
    var authorizationCode = new AuthorizationCode
    {
      AuthorizationGrantId = authorizationGrantId,
      AuthorizationCodeId = authorizationCodeId,
      NonceId = nonceId,
      CodeChallenge = codeChallenge,
      CodeChallengeMethod = codeChallengeMethod,
      Scopes = scopes
    };
    writer.Write(JsonSerializer.Serialize(authorizationCode));
    var protectedBytes = _dataProtector.Protect(ms.ToArray());
    return Base64UrlEncoder.Encode(protectedBytes);
  }
}

public class AuthorizationCode
{
  public string AuthorizationGrantId { get; set; } = null!;
  public string AuthorizationCodeId { get; set; } = null!;
  public string NonceId { get; set; } = null!;
  public string CodeChallenge { get; set; } = null!;
  public string CodeChallengeMethod { get; set; } = null!;
  public ICollection<string> Scopes { get; set; } = new List<string>();
}