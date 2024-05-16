using AuthServer.Core;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repositories;
internal class ConsentGrantRepository : IConsentGrantRepository
{
    private readonly IdentityContext _identityContext;

    public ConsentGrantRepository(IdentityContext identityContext)
    {
        _identityContext = identityContext;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<string>> GetConsentedScope(string publicSubjectIdentifier, string clientId, CancellationToken cancellationToken)
    {
        return await _identityContext
            .Set<ConsentGrant>()
            .Where(cg => cg.PublicSubjectIdentifier.Id == publicSubjectIdentifier)
            .Where(cg => cg.Client.Id == clientId)
            .SelectMany(cg => cg.ConsentedScopes)
            .Select(s => s.Name)
            .ToListAsync(cancellationToken);
    }
}