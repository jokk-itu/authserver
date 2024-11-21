using System.Diagnostics;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Metrics;
using AuthServer.Metrics.Abstractions;
using AuthServer.Options;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.Extensions.Options;

namespace AuthServer.TokenBuilders;
internal class RegistrationTokenBuilder : ITokenBuilder<RegistrationTokenArguments>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IMetricService _metricService;

    public RegistrationTokenBuilder(
        AuthorizationDbContext identityContext,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IMetricService metricService)
    {
        _identityContext = identityContext;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _metricService = metricService;
    }

    public async Task<string> BuildToken(RegistrationTokenArguments arguments, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var client = (await _identityContext.FindAsync<Client>([arguments.ClientId], cancellationToken))!;

        var registrationToken = new RegistrationToken(client, arguments.ClientId,
            _discoveryDocumentOptions.Value.Issuer, ScopeConstants.Register);

        await _identityContext
            .Set<RegistrationToken>()
            .AddAsync(registrationToken, cancellationToken);

        stopWatch.Stop();
        _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.RegistrationToken, TokenStructureTag.Reference);

        return registrationToken.Reference;
    }
}