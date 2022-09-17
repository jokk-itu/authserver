using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
public class CodeManager
{
  private readonly IdentityContext _identityContext;

  public CodeManager(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<bool> CreateAuthorizationCodeAsync(Client client, string encryptedCode, CancellationToken cancellationToken = default)
  {
    var code = new Code
    {
      Client = client,
      CodeType = CodeType.AuthorizationCode,
      Value = encryptedCode,
      IsRedeemed = false
    };
    await _identityContext.Set<Code>().AddAsync(code, cancellationToken);
    var result = await _identityContext.SaveChangesAsync(cancellationToken);
    return result > 0;
  }

  public async Task<bool> RedeemCodeAsync(Code code, CancellationToken cancellationToken = default)
  {
    code.IsRedeemed = true;
    var result = await _identityContext.SaveChangesAsync(cancellationToken);
    return result > 0;
  }

  public async Task<Code?> ReadCodeAsync(string encryptedCode, CancellationToken cancellationToken = default) 
  {
    return await _identityContext
      .Set<Code>()
      .SingleOrDefaultAsync(code => code.Value == encryptedCode, cancellationToken: cancellationToken);
  }
}