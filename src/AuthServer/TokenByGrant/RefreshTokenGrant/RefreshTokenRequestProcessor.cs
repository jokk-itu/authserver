using AuthServer.Cache.Abstractions;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;

namespace AuthServer.TokenByGrant.RefreshTokenGrant;

internal class RefreshTokenRequestProcessor : IRequestProcessor<RefreshTokenValidatedRequest, TokenResponse>
{
    private readonly ITokenBuilder<GrantAccessTokenArguments> _accessTokenBuilder;
    private readonly ITokenBuilder<RefreshTokenArguments> _refreshTokenBuilder;
    private readonly ITokenBuilder<IdTokenArguments> _idTokenBuilder;
    private readonly ICachedClientStore _cachedEntityStore;

    public RefreshTokenRequestProcessor(
        ITokenBuilder<GrantAccessTokenArguments> accessTokenBuilder,
        ITokenBuilder<RefreshTokenArguments> refreshTokenBuilder,
        ITokenBuilder<IdTokenArguments> idTokenBuilder,
        ICachedClientStore cachedEntityStore)
    {
        _accessTokenBuilder = accessTokenBuilder;
        _refreshTokenBuilder = refreshTokenBuilder;
        _idTokenBuilder = idTokenBuilder;
        _cachedEntityStore = cachedEntityStore;
    }

    public async Task<TokenResponse> Process(RefreshTokenValidatedRequest request, CancellationToken cancellationToken)
    {
        var cachedClient = await _cachedEntityStore.Get(request.ClientId, cancellationToken);
        var accessToken = await _accessTokenBuilder.BuildToken(new GrantAccessTokenArguments
        {
            AuthorizationGrantId = request.AuthorizationGrantId,
            Resource = request.Resource,
            Scope = request.Scope
        }, cancellationToken);

        var idToken = await _idTokenBuilder.BuildToken(new IdTokenArguments
        {
            AuthorizationGrantId = request.AuthorizationGrantId,
            Scope = request.Scope
        }, cancellationToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            IdToken = idToken,
            Scope = string.Join(' ', request.Scope),
            ExpiresIn = cachedClient.AccessTokenExpiration
        };
    }
}