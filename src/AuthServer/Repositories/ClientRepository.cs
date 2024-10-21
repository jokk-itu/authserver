using System.Text.Json;
using AuthServer.Authorization;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repositories;

internal class ClientRepository : IClientRepository
{
    private readonly AuthorizationDbContext _authorizationDbContext;

    public ClientRepository(
        AuthorizationDbContext authorizationDbContext)
    {
        _authorizationDbContext = authorizationDbContext;
    }

    /// <inheritdoc/>
    public async Task<bool> DoesResourcesExist(IReadOnlyCollection<string> resources,
        IReadOnlyCollection<string> scopes, CancellationToken cancellationToken)
    {
        var resourcesExisting = await _authorizationDbContext
            .Set<Client>()
            .Where(r => r.ClientUri != null && resources.Contains(r.ClientUri))
            .Where(r => r.Scopes.AsQueryable().Any(s => scopes.Contains(s.Name)))
            .CountAsync(cancellationToken: cancellationToken);

        return resourcesExisting == resources.Count;
    }

    /// <inheritdoc/>
    public async Task<AuthorizeRequestDto?> GetAuthorizeDto(string reference, string clientId,
        CancellationToken cancellationToken)
    {
        var authorizeMessage = await _authorizationDbContext
            .Set<Client>()
            .Where(x => x.Id == clientId)
            .SelectMany(x => x.AuthorizeMessages)
            .Where(AuthorizeMessage.IsActive)
            .Where(x => x.Reference == reference)
            .SingleOrDefaultAsync(cancellationToken);

        if (authorizeMessage is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<AuthorizeRequestDto>(authorizeMessage.Value);
    }

    /// <inheritdoc/>
    public async Task<AuthorizeMessage> AddAuthorizeMessage(AuthorizeRequestDto authorizeDto, CancellationToken cancellationToken)
    {
        var value = JsonSerializer.Serialize(authorizeDto);
        var client = (await _authorizationDbContext.FindAsync<Client>([authorizeDto.ClientId], cancellationToken))!;
        var authorizeMessage = new AuthorizeMessage(value, DateTime.UtcNow.AddSeconds(client.RequestUriExpiration!.Value), client);
        await _authorizationDbContext.AddAsync(authorizeMessage, cancellationToken);
        return authorizeMessage;
    }

    /// <inheritdoc/>
    public async Task RedeemAuthorizeMessage(string reference, CancellationToken cancellationToken)
    {
        var authorizeMessage = await _authorizationDbContext
            .Set<AuthorizeMessage>()
            .Where(x => x.Reference == reference)
            .SingleAsync(cancellationToken);

        authorizeMessage.Redeem();
    }
}