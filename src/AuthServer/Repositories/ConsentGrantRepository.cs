﻿using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repositories;
internal class ConsentGrantRepository : IConsentGrantRepository
{
    private readonly AuthorizationDbContext _identityContext;

    public ConsentGrantRepository(AuthorizationDbContext identityContext)
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

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<string>> GetConsentedClaims(string publicSubjectIdentifier, string clientId, CancellationToken cancellationToken)
    {
        var client = await _identityContext.FindAsync<Client>(clientId);
        if (!client!.RequireConsent)
        {
            return await _identityContext
                .Set<Claim>()
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        return await _identityContext
            .Set<ConsentGrant>()
            .Where(cg => cg.PublicSubjectIdentifier.Id == publicSubjectIdentifier)
            .Where(cg => cg.Client.Id == clientId)
            .SelectMany(cg => cg.ConsentedClaims)
            .Select(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateConsentGrant(string subjectIdentifier, string clientId, IEnumerable<string> scope, IEnumerable<string> claims,
        CancellationToken cancellationToken)
    {
        var activeConsentGrant = await GetConsentGrant(subjectIdentifier, clientId, cancellationToken);
        if (activeConsentGrant is null)
        {
            var publicSubjectIdentifier = (await _identityContext.FindAsync<PublicSubjectIdentifier>(subjectIdentifier, cancellationToken))!;
            var client = (await _identityContext.FindAsync<Client>(clientId, cancellationToken))!;
            activeConsentGrant = new ConsentGrant(publicSubjectIdentifier, client);
            await _identityContext.AddAsync(activeConsentGrant, cancellationToken);
        }

        activeConsentGrant.ConsentedClaims.Clear();
        activeConsentGrant.ConsentedScopes.Clear();

        (await _identityContext
            .Set<Claim>()
            .Where(x => claims.Contains(x.Name))
            .ToListAsync(cancellationToken))
            .ForEach(activeConsentGrant.ConsentedClaims.Add);

        (await _identityContext
            .Set<Scope>()
            .Where(x => scope.Contains(x.Name))
            .ToListAsync(cancellationToken))
            .ForEach(activeConsentGrant.ConsentedScopes.Add);

        await _identityContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ConsentGrant?> GetConsentGrant(string subjectIdentifier, string clientId, CancellationToken cancellationToken)
    {
        return await _identityContext
            .Set<ConsentGrant>()
            .Where(x => x.PublicSubjectIdentifier.Id == subjectIdentifier)
            .Where(x => x.Client.Id == clientId)
            .Include(x => x.ConsentedClaims)
            .Include(x => x.ConsentedScopes)
            .SingleOrDefaultAsync(cancellationToken);
    }
}