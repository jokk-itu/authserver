namespace AuthServer.TokenByGrant.ClientCredentialsGrant;
internal interface IClientCredentialsProcessor
{
    Task<TokenResponse> Process(ClientCredentialsValidatedRequest request, CancellationToken cancellationToken);
}