using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.TokenDecoders;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Revocation;
internal class TokenRevoker : ITokenRevoker
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;

    public TokenRevoker(
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder)
    {
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
    }

    public async Task Revoke(RevocationValidatedRequest request, CancellationToken cancellationToken)
    {
        var token = await GetToken(request, cancellationToken);
        token?.Revoke();
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

        var jsonWebToken = await _serverIssuedTokenDecoder.Read(request.Token);
        var id = Guid.Parse(jsonWebToken.Id);

        return await _identityContext
            .Set<Token>()
            .Where(x => x.RevokedAt == null)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }
}