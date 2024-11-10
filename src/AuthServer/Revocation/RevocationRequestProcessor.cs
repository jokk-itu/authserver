﻿using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AuthServer.Revocation;
internal class RevocationRequestProcessor : IRequestProcessor<RevocationValidatedRequest, Unit>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;

    public RevocationRequestProcessor(
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder)
    {
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
    }

    public async Task<Unit> Process(RevocationValidatedRequest request, CancellationToken cancellationToken)
    {
        var token = await GetToken(request, cancellationToken);
        token?.Revoke();
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