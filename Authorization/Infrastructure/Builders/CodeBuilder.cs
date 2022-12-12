using System.Text;
using System.Text.Json;
using Application;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
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
    string codeChallenge, 
    string codeChallengeMethod,
    string userId,
    string clientId,
    ICollection<string> scopes)
  {
    var ms = new MemoryStream();
    await using var writer = new BinaryWriter(ms, Encoding.UTF8, false);
    var authorizationCode = new AuthorizationCode
    {
      CodeChallenge = codeChallenge,
      CodeChallengeMethod = codeChallengeMethod,
      AuthorizationGrantId = authorizationGrantId,
      ClientId = clientId,
      UserId = userId,
      Scopes = scopes
    };
    writer.Write(JsonSerializer.Serialize(authorizationCode));
    var protectedBytes = _dataProtector.Protect(ms.ToArray());
    return Base64UrlEncoder.Encode(protectedBytes);
  }

  public async Task<string> BuildLoginCodeAsync(string userId)
  {
    var ms = new MemoryStream();
    await using var writer = new BinaryWriter(ms, Encoding.UTF8, false);
    var loginCode = new LoginCode
    {
      UserId = userId,
      Random = CryptographyHelper.GetRandomString(16)
    };
    writer.Write(JsonSerializer.Serialize(loginCode));
    var protectedBytes = _dataProtector.Protect(ms.ToArray());
    return Base64UrlEncoder.Encode(protectedBytes);
  }
}

public class LoginCode
{
  public string UserId { get; set; } = null!;
  public string Random { get; set; } = null!;
}

public class AuthorizationCode
{
  public string AuthorizationGrantId { get; set; } = null!;
  public string CodeChallenge { get; set; } = null!;
  public string CodeChallengeMethod { get; set; } = null!;
  public string ClientId { get; set; } = null!;
  public string UserId { get; set; } = null!;
  public ICollection<string> Scopes { get; set; } = new List<string>();
}