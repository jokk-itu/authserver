using AuthServer.Authorize.Abstractions;
using AuthServer.Repositories.Abstractions;

namespace AuthServer.Authorize;
internal class AuthorizeService : IAuthorizeService
{
    private readonly IConsentGrantRepository _consentGrantRepository;
    private readonly IAuthorizationGrantRepository _authorizationGrantRepository;

    public AuthorizeService(
        IConsentGrantRepository consentGrantRepository,
        IAuthorizationGrantRepository authorizationGrantRepository)
    {
        _consentGrantRepository = consentGrantRepository;
        _authorizationGrantRepository = authorizationGrantRepository;
    }

    /// <inheritdoc/>
    public async Task CreateAuthorizationGrant(string subjectIdentifier, string clientId, long? maxAge,
        CancellationToken cancellationToken)
    {
        await _authorizationGrantRepository.CreateAuthorizationGrant(subjectIdentifier, clientId, maxAge, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateConsentGrant(string subjectIdentifier, string clientId, ConsentGrantDto consentGrantDto,
        CancellationToken cancellationToken)
    {
        await _consentGrantRepository.CreateOrUpdateConsentGrant(subjectIdentifier, clientId, consentGrantDto.Scope, consentGrantDto.Claims, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ConsentGrantDto?> GetConsentGrantDto(string subjectIdentifier, string clientId, CancellationToken cancellationToken)
    {
        var consentGrant = await _consentGrantRepository.GetConsentGrant(subjectIdentifier, clientId, cancellationToken);
        if (consentGrant is null)
        {
            return null;
        }

        return new ConsentGrantDto(
            consentGrant.ConsentedScopes.Select(x => x.Name),
            consentGrant.ConsentedClaims.Select(x => x.Name));
    }
}
