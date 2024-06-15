namespace AuthServer.Revocation.Abstractions;
internal interface ITokenRevoker
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Revoke(RevocationValidatedRequest request, CancellationToken cancellationToken);
}