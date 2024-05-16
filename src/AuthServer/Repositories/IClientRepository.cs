namespace AuthServer.Repositories;
internal interface IClientRepository
{
    /// <summary>
    /// Returns whether resources map to existing scope.
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="scopes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> DoesResourcesExist(IReadOnlyCollection<string> resources, IReadOnlyCollection<string> scopes, CancellationToken cancellationToken);
}