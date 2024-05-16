namespace AuthServer.TokenByGrant.RefreshTokenGrant;

internal interface IRefreshTokenProcessor
{
    Task<TokenResponse> Process(RefreshTokenValidatedRequest request, CancellationToken cancellationToken);
}