namespace AuthServer.Authorize.Abstractions;

public interface IAuthorizeService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="maxAge"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateAuthorizationGrant(string subjectIdentifier, string clientId, long? maxAge, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="consentGrantDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateOrUpdateConsentGrant(string subjectIdentifier, string clientId, ConsentGrantDto consentGrantDto, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ConsentGrantDto?> GetConsentGrantDto(string subjectIdentifier, string clientId, CancellationToken cancellationToken);
}