namespace AuthServer.Repositories.Abstractions;
internal interface INonceRepository
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="nonce"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsNonceReplay(string nonce, CancellationToken cancellationToken);
}