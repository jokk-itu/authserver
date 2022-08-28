using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
public class NonceManager
{
  private readonly IdentityContext _identityContext;

  public NonceManager(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<bool> CreateNonceAsync(string nonceValue, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(nonceValue))
      throw new ArgumentException("Must not be empty or whitespace", nameof(nonceValue));

    var nonce = new Nonce
    {
      Value = nonceValue
    };

    await _identityContext
      .Set<Nonce>()
      .AddAsync(nonce, cancellationToken);

    var result = await _identityContext.SaveChangesAsync(cancellationToken);
    return result > 0;
  }

  public async Task<Nonce?> ReadNonceAsync(string nonceValue, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(nonceValue))
      throw new ArgumentException("Must not be empty or whitespace", nameof(nonceValue));

    return await _identityContext
      .Set<Nonce>()
      .SingleOrDefaultAsync(nonce => nonce.Value == nonceValue, cancellationToken: cancellationToken);
  }
}
