using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.TokenByGrant.AuthorizationCodeGrant;
internal class AuthorizationCodeRequestProcessor : IRequestProcessor<AuthorizationCodeValidatedRequest, TokenResponse>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly ITokenBuilder<GrantAccessTokenArguments> _accessTokenBuilder;
    private readonly ITokenBuilder<RefreshTokenArguments> _refreshTokenBuilder;
    private readonly ITokenBuilder<IdTokenArguments> _idTokenBuilder;

    public AuthorizationCodeRequestProcessor(
        AuthorizationDbContext identityContext,
        ICachedClientStore cachedClientStore,
        ITokenBuilder<GrantAccessTokenArguments> accessTokenBuilder,
        ITokenBuilder<RefreshTokenArguments> refreshTokenBuilder,
        ITokenBuilder<IdTokenArguments> idTokenBuilder)
    {
        _identityContext = identityContext;
        _cachedClientStore = cachedClientStore;
        _accessTokenBuilder = accessTokenBuilder;
        _refreshTokenBuilder = refreshTokenBuilder;
        _idTokenBuilder = idTokenBuilder;
    }

    public async Task<TokenResponse> Process(AuthorizationCodeValidatedRequest request, CancellationToken cancellationToken)
    {
        var query = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == request.AuthorizationGrantId)
            .Select(x => new
            {
                AuthorizationCodeGrant = x,
                ClientId = x.Client.Id,
                AuthorizationCode = x.AuthorizationCodes.Single(y => y.Id == request.AuthorizationCodeId)
            })
            .SingleAsync(cancellationToken: cancellationToken);

        query.AuthorizationCode.Redeem();

        var cachedClient = await _cachedClientStore.Get(query.ClientId, cancellationToken);

        string? refreshToken = null;
        if (cachedClient.GrantTypes.Any(x => x == GrantTypeConstants.RefreshToken))
        {
            refreshToken = await _refreshTokenBuilder.BuildToken(new RefreshTokenArguments
            {
                AuthorizationGrantId = query.AuthorizationCodeGrant.Id,
                Scope = request.Scope
            }, cancellationToken);
        }

        var accessToken = await _accessTokenBuilder.BuildToken(new GrantAccessTokenArguments
        {
            AuthorizationGrantId = query.AuthorizationCodeGrant.Id,
            Scope = request.Scope,
            Resource = request.Resource
        }, cancellationToken);

        var idToken = await _idTokenBuilder.BuildToken(new IdTokenArguments
        {
            AuthorizationGrantId = query.AuthorizationCodeGrant.Id,
            Scope = request.Scope
        }, cancellationToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            IdToken = idToken,
            RefreshToken = refreshToken,
            ExpiresIn = cachedClient.AccessTokenExpiration,
            Scope = string.Join(' ', request.Scope),
        };
    }
}