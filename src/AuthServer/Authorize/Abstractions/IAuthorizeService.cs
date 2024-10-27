namespace AuthServer.Authorize.Abstractions;

public interface IAuthorizeService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="amr"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateAuthorizationGrant(string subjectIdentifier, string clientId, IReadOnlyCollection<string> amr, CancellationToken cancellationToken);
     
    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="consentedClaims"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="consentedScope"></param>
    /// <returns></returns>
    Task CreateOrUpdateConsentGrant(string subjectIdentifier, string clientId, IEnumerable<string> consentedScope, IEnumerable<string> consentedClaims, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ConsentGrantDto> GetConsentGrantDto(string subjectIdentifier, string clientId, CancellationToken cancellationToken);
}