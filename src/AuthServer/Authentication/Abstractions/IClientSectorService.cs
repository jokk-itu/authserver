namespace AuthServer.Authentication.Abstractions;
internal interface IClientSectorService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sectorIdentifierUri"></param>
    /// <param name="uris"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ContainsSectorDocument(Uri sectorIdentifierUri, IReadOnlyCollection<string> uris, CancellationToken cancellationToken);
}
