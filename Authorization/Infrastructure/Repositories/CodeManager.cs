using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;
public class CodeManager
{
  private readonly IdentityContext _identityContext;
  private readonly ClientManager _clientManager;
  private readonly ILogger<CodeManager> _logger;

  public CodeManager(IdentityContext identityContext, ClientManager clientManager, ILogger<CodeManager> logger)
  {
    _identityContext = identityContext;
    _clientManager = clientManager;
    _logger = logger;
  }

  public async Task<bool> CreateAuthorizationCodeAsync(Client client, string encryptedCode, CancellationToken cancellationToken = default)
  {
    var codeType = await _identityContext
      .Set<CodeType>()
      .SingleAsync(codeType => codeType.Name == Domain.Constants.CodeTypeConstants.AuthorizationCode, cancellationToken: cancellationToken);

    var code = new Code
    {
      Client = client,
      CodeType = codeType,
      Value = encryptedCode
    };
    await _identityContext.Set<Code>().AddAsync(code, cancellationToken);
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