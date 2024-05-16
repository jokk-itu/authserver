using AuthServer.Core;
using AuthServer.Core.Discovery;
using AuthServer.Entities;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthServer.TokenBuilders;
internal class RegistrationTokenBuilder : ITokenBuilder<RegistrationTokenArguments>
{
    private readonly IdentityContext _identityContext;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    public RegistrationTokenBuilder(
        IdentityContext identityContext,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _identityContext = identityContext;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public async Task<string> BuildToken(RegistrationTokenArguments arguments, CancellationToken cancellationToken)
    {
        var client = await _identityContext
            .Set<Client>()
            .SingleAsync(x => x.Id == arguments.ClientId, cancellationToken);

        var registrationToken = new RegistrationToken(client, arguments.ClientId,
            _discoveryDocumentOptions.Value.Issuer, null, null);

        await _identityContext
            .Set<RegistrationToken>()
            .AddAsync(registrationToken, cancellationToken);

        return registrationToken.Reference;
    }
}