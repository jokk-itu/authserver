namespace AuthServer.TokenByGrant.AuthorizationCodeGrant;
internal interface IAuthorizationCodeProcessor
{
    Task<TokenResponse> Process(AuthorizationCodeValidatedRequest request, CancellationToken cancellationToken);
}