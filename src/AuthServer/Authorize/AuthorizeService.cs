using AuthServer.Authentication.Abstractions;
using AuthServer.Authorize.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Metrics.Abstractions;
using AuthServer.Repositories.Abstractions;

namespace AuthServer.Authorize;
internal class AuthorizeService : IAuthorizeService
{
    private readonly IConsentGrantRepository _consentGrantRepository;
    private readonly IAuthorizationGrantRepository _authorizationGrantRepository;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IUserClaimService _userClaimService;
    private readonly IMetricService _metricService;
    private readonly IAuthenticationContextReferenceResolver _authenticationContextResolver;

    public AuthorizeService(
        IConsentGrantRepository consentGrantRepository,
        IAuthorizationGrantRepository authorizationGrantRepository,
        ICachedClientStore cachedClientStore,
        IUserClaimService userClaimService,
        IMetricService metricService,
        IAuthenticationContextReferenceResolver authenticationContextResolver)
    {
        _consentGrantRepository = consentGrantRepository;
        _authorizationGrantRepository = authorizationGrantRepository;
        _cachedClientStore = cachedClientStore;
        _userClaimService = userClaimService;
        _metricService = metricService;
        _authenticationContextResolver = authenticationContextResolver;
    }

    /// <inheritdoc/>
    public async Task CreateAuthorizationGrant(string subjectIdentifier, string clientId, IReadOnlyCollection<string> amr,
        CancellationToken cancellationToken)
    {
        var acr = await _authenticationContextResolver.ResolveAuthenticationContextReference(amr, cancellationToken);
        await _authorizationGrantRepository.CreateAuthorizationGrant(subjectIdentifier, clientId, acr, amr, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateConsentGrant(string subjectIdentifier, string clientId, IEnumerable<string> consentedScope, IEnumerable<string> consentedClaims,
        CancellationToken cancellationToken)
    {
        await _consentGrantRepository.CreateOrUpdateConsentGrant(subjectIdentifier, clientId, consentedScope, consentedClaims, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ConsentGrantDto> GetConsentGrantDto(string subjectIdentifier, string clientId, CancellationToken cancellationToken)
    {
        var consentGrant = await _consentGrantRepository.GetConsentGrant(subjectIdentifier, clientId, cancellationToken);
        var cachedClient = await _cachedClientStore.Get(clientId, cancellationToken);
        var username = await _userClaimService.GetUsername(subjectIdentifier, cancellationToken);

        return new ConsentGrantDto
        {
            ClientName = cachedClient.Name,
            ClientLogoUri = cachedClient.LogoUri,
            ClientUri = cachedClient.ClientUri,
            Username = username,
            ConsentedScope = consentGrant?.ConsentedScopes.Select(x => x.Name) ?? [],
            ConsentedClaims = consentGrant?.ConsentedClaims.Select(x => x.Name) ?? []
        };
    }
}
