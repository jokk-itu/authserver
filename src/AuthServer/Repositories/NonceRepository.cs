using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repositories;
internal class NonceRepository : INonceRepository
{
    private readonly AuthorizationDbContext _authorizationDbContext;

    public NonceRepository(AuthorizationDbContext authorizationDbContext)
    {
        _authorizationDbContext = authorizationDbContext;
    }

    /// <inheritdoc/>
    public async Task<bool> IsNonceReplay(string nonce, CancellationToken cancellationToken)
    {
        var hashedNonce = nonce.Sha256();
        return await _authorizationDbContext
            .Set<Nonce>()
            .AnyAsync(x => x.HashedValue == hashedNonce, cancellationToken);
    }
}
