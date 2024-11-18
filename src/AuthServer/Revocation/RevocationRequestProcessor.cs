using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.Metrics;
using AuthServer.Metrics.Abstractions;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AuthServer.Revocation;
internal class RevocationRequestProcessor : IRequestProcessor<RevocationValidatedRequest, Unit>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IMetricService _metricService;

    public RevocationRequestProcessor(
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IMetricService metricService)
    {
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _metricService = metricService;
    }

    public async Task<Unit> Process(RevocationValidatedRequest request, CancellationToken cancellationToken)
    {
        var token = await GetToken(request, cancellationToken);
        if (token is not null)
        {
            _metricService.AddRevokedToken(token is RefreshToken ? TokenTypeTag.RefreshToken : TokenTypeTag.AccessToken);
            token.Revoke();
        }

        return Unit.Value;
    }

    private async Task<Token?> GetToken(RevocationValidatedRequest request, CancellationToken cancellationToken)
    {
        if (!TokenHelper.IsJws(request.Token))
        {
            return await _identityContext
                .Set<Token>()
                .Where(x => x.RevokedAt == null)
                .SingleOrDefaultAsync(x => x.Reference == request.Token,
                    cancellationToken: cancellationToken);
        }

        JsonWebToken jsonWebToken;
        try
        {
            jsonWebToken = await _serverIssuedTokenDecoder.Read(request.Token);
        }
        catch
        {
            // if the token is invalid, then it is ignored per rfc 7009
            return null;
        }

        var id = Guid.Parse(jsonWebToken.Id);
        return await _identityContext
            .Set<Token>()
            .Where(x => x.RevokedAt == null)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }
}