using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.Options;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Register;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Register;

internal class RegisterRequestValidator : IRequestValidator<RegisterRequest, RegisterValidatedRequest>
{
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly IClientJwkService _clientJwkService;
    private readonly ILogger<RegisterRequestValidator> _logger;
    private readonly ITokenRepository _tokenRepository;
    private readonly IClientSectorService _clientSectorService;

    private DiscoveryDocument DiscoveryDocument => _discoveryDocumentOptions.Value;

    public RegisterRequestValidator(
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        AuthorizationDbContext authorizationDbContext,
        IClientJwkService clientJwkService,
        ILogger<RegisterRequestValidator> logger,
        ITokenRepository tokenRepository,
        IClientSectorService clientSectorService)
    {
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _authorizationDbContext = authorizationDbContext;
        _clientJwkService = clientJwkService;
        _logger = logger;
        _tokenRepository = tokenRepository;
        _clientSectorService = clientSectorService;
    }

    public async Task<ProcessResult<RegisterValidatedRequest, ProcessError>> Validate(RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validatedRequest = new RegisterValidatedRequest
        {
            Method = request.Method
        };

        var managementError = await ValidateManagementParameters(request, validatedRequest, cancellationToken);
        if (managementError is not null)
        {
            return managementError;
        }

        // Delete and Get only uses the Management parameters
        if (request.Method == HttpMethod.Delete || request.Method == HttpMethod.Get)
        {
            return validatedRequest;
        }

        var applicationTypeError = ValidateApplicationType(request, validatedRequest);
        if (applicationTypeError is not null)
        {
            return applicationTypeError;
        }

        var tokenEndpointAuthMethod = ValidateTokenEndpointAuthMethod(request, validatedRequest);
        if (tokenEndpointAuthMethod is not null)
        {
            return tokenEndpointAuthMethod;
        }

        var clientNameError = await ValidateClientName(request, validatedRequest, cancellationToken);
        if (clientNameError is not null)
        {
            return clientNameError;
        }

        var grantTypesError = ValidateGrantTypes(request, validatedRequest);
        if (grantTypesError is not null)
        {
            return grantTypesError;
        }

        var scopeError = ValidateScope(request, validatedRequest);
        if (scopeError is not null)
        {
            return scopeError;
        }

        var responseTypesError = ValidateResponseTypes(request, validatedRequest);
        if (responseTypesError is not null)
        {
            return responseTypesError;
        }

        var subjectTypeError = ValidateSubjectType(request, validatedRequest);
        if (subjectTypeError is not null)
        {
            return subjectTypeError;
        }

        var urisError = await ValidateUris(request, validatedRequest, cancellationToken);
        if (urisError is not null)
        {
            return urisError;
        }

        var requireSignedRequestObjectError = ValidateRequireSignedRequestObject(request, validatedRequest);
        if (requireSignedRequestObjectError is not null)
        {
            return requireSignedRequestObjectError;
        }

        var requireReferenceTokenError = ValidateRequireReferenceToken(request, validatedRequest);
        if (requireReferenceTokenError is not null)
        {
            return requireReferenceTokenError;
        }

        var requirePushedAuthorizationRequestsError = ValidateRequirePushedAuthorizationRequests(request, validatedRequest);
        if (requirePushedAuthorizationRequestsError is not null)
        {
            return requirePushedAuthorizationRequestsError;
        }

        var defaultMaxAgeError = ValidateDefaultMaxAge(request, validatedRequest);
        if (defaultMaxAgeError is not null)
        {
            return RegisterError.InvalidDefaultMaxAge;
        }

        var defaultAcrValuesError = ValidateDefaultAcrValues(request, validatedRequest);
        if (defaultAcrValuesError is not null)
        {
            return defaultAcrValuesError;
        }

        var contactsError = ValidateContacts(request, validatedRequest);
        if (contactsError is not null)
        {
            return contactsError;
        }

        var authorizationCodeExpirationError = ValidateAuthorizationCodeExpiration(request, validatedRequest);
        if (authorizationCodeExpirationError is not null)
        {
            return authorizationCodeExpirationError;
        }

        var accessTokenExpirationError = ValidateAccessTokenExpiration(request, validatedRequest);
        if (accessTokenExpirationError is not null)
        {
            return accessTokenExpirationError;
        }

        var refreshTokenExpirationError = ValidateRefreshTokenExpiration(request, validatedRequest);
        if (refreshTokenExpirationError is not null)
        {
            return refreshTokenExpirationError;
        }

        var clientSecretExpirationError = ValidateClientSecretExpiration(request, validatedRequest);
        if (clientSecretExpirationError is not null)
        {
            return clientSecretExpirationError;
        }

        var jwksExpirationError = ValidateJwksExpiration(request, validatedRequest);
        if (jwksExpirationError is not null)
        {
            return jwksExpirationError;
        }

        var requestUriExpirationError = ValidateRequestUriExpiration(request, validatedRequest);
        if (requestUriExpirationError is not null)
        {
            return requestUriExpirationError;
        }

        var tokenEndpointAuthSigningAlgError = ValidateTokenEndpointAuthSigningAlg(request, validatedRequest);
        if (tokenEndpointAuthSigningAlgError is not null)
        {
            return tokenEndpointAuthSigningAlgError;
        }

        var requestObjectTokenError = ValidateRequestObjectToken(request, validatedRequest);
        if (requestObjectTokenError is not null)
        {
            return requestObjectTokenError;
        }

        var userinfoResponseToken = ValidateUserinfoResponseToken(request, validatedRequest);
        if (userinfoResponseToken is not null)
        {
            return userinfoResponseToken;
        }

        var idTokenError = ValidateIdToken(request, validatedRequest);
        if (idTokenError is not null)
        {
            return idTokenError;
        }

        return validatedRequest;
    }

    private async Task<ProcessError?> ValidateUris(RegisterRequest request, RegisterValidatedRequest validatedRequest, CancellationToken cancellationToken)
    {
        var redirectUrisError = ValidateRedirectUris(request, validatedRequest);
        if (redirectUrisError is not null)
        {
            return redirectUrisError;
        }

        var postLogoutRedirectUrisError = ValidatePostLogoutRedirectUris(request, validatedRequest);
        if (postLogoutRedirectUrisError is not null)
        {
            return postLogoutRedirectUrisError;
        }

        var requestUrisError = ValidateRequestUris(request, validatedRequest);
        if (requestUrisError is not null)
        {
            return requestUrisError;
        }

        var sectorIdentifierUri = await ValidateSectorIdentifierUri(request, validatedRequest, cancellationToken);
        if (sectorIdentifierUri is not null)
        {
            return sectorIdentifierUri;
        }

        var backchannelLogoutUriError = ValidateBackchannelLogoutUri(request, validatedRequest);
        if (backchannelLogoutUriError is not null)
        {
            return backchannelLogoutUriError;
        }

        var clientUriError = ValidateClientUri(request, validatedRequest);
        if (clientUriError is not null)
        {
            return clientUriError;
        }

        var policyUriError = ValidatePolicyUri(request, validatedRequest);
        if (policyUriError is not null)
        {
            return policyUriError;
        }

        var tosUriError = ValidateTosUri(request, validatedRequest);
        if (tosUriError is not null)
        {
            return tosUriError;
        }

        var initiateLoginUriError = ValidateInitiateLoginUri(request, validatedRequest);
        if (initiateLoginUriError is not null)
        {
            return initiateLoginUriError;
        }

        var logoUriError = ValidateLogoUri(request, validatedRequest);
        if (logoUriError is not null)
        {
            return logoUriError;
        }

        if (!string.IsNullOrEmpty(request.Jwks) && !string.IsNullOrEmpty(request.JwksUri))
        {
            return RegisterError.InvalidJwksAndJwksUri;
        }

        if (string.IsNullOrEmpty(request.Jwks) && string.IsNullOrEmpty(request.JwksUri) &&
            validatedRequest.TokenEndpointAuthMethod == TokenEndpointAuthMethod.PrivateKeyJwt)
        {
            return RegisterError.InvalidJwksOrJwksUri;
        }

        var jwksError = ValidateJwks(request, validatedRequest);
        if (jwksError is not null)
        {
            return jwksError;
        }

        var jwksUriError = await ValidateJwksUri(request, validatedRequest, cancellationToken);
        if (jwksUriError is not null)
        {
            return jwksUriError;
        }

        return null;
    }

    /// <summary>
    /// ApplicationType is OPTIONAL.
    /// Default value is set to <see cref="ApplicationType.Web"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateApplicationType(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (!string.IsNullOrEmpty(request.ApplicationType)
            && !ApplicationTypeConstants.ApplicationTypes.Contains(request.ApplicationType))
        {
            return RegisterError.InvalidApplicationType;
        }

        validatedRequest.ApplicationType = string.IsNullOrEmpty(request.ApplicationType)
            ? ApplicationType.Web
            : request.ApplicationType.GetEnum<ApplicationType>();

        return null;
    }

    /// <summary>
    /// TokenEndpointAuthMethod is OPTIONAL.
    /// Default value is <see cref="TokenEndpointAuthMethod.ClientSecretBasic"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateTokenEndpointAuthMethod(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (!string.IsNullOrEmpty(request.TokenEndpointAuthMethod)
            && !DiscoveryDocument.TokenEndpointAuthMethodsSupported.Contains(request.TokenEndpointAuthMethod))
        {
            return RegisterError.InvalidTokenEndpointAuthMethod;
        }

        validatedRequest.TokenEndpointAuthMethod = string.IsNullOrEmpty(request.TokenEndpointAuthMethod)
            ? TokenEndpointAuthMethod.ClientSecretBasic
            : request.TokenEndpointAuthMethod.GetEnum<TokenEndpointAuthMethod>();

        return null;
    }

    /// <summary>
    /// ClientName is REQUIRED.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<ProcessError?> ValidateClientName(RegisterRequest request,
        RegisterValidatedRequest validatedRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ClientName))
        {
            return RegisterError.InvalidClientName;
        }

        var clientId = await _authorizationDbContext
            .Set<Client>()
            .Where(x => x.Name == request.ClientName)
            .Select(x => x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrEmpty(clientId) && clientId != request.ClientId)
        {
            return RegisterError.InvalidClientName;
        }

        validatedRequest.ClientName = request.ClientName;
        return null;
    }

    /// <summary>
    /// GrantTypes is OPTIONAL.
    /// Default value is <see cref="GrantTypeConstants.AuthorizationCode"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateGrantTypes(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (request.GrantTypes.Count != 0
            && request.GrantTypes.ExceptAny(DiscoveryDocument.GrantTypesSupported))
        {
            return RegisterError.InvalidGrantTypes;
        }

        if (request.GrantTypes.Contains(GrantTypeConstants.RefreshToken)
            && !request.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            return RegisterError.InvalidGrantTypes;
        }

        validatedRequest.GrantTypes = request.GrantTypes.Count == 0
            ? [GrantTypeConstants.AuthorizationCode]
            : request.GrantTypes;

        return null;
    }

    /// <summary>
    /// Scope is OPTIONAL.
    /// Default is <see cref="ScopeConstants.OpenId"/> if GrantTypes are OpenIdConnect compliant.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateScope(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (request.Scope.Count != 0)
        {
            if (request.Scope.ExceptAny(DiscoveryDocument.ScopesSupported))
            {
                return RegisterError.InvalidScope;
            }
            validatedRequest.Scope = request.Scope;
        }
        else if (validatedRequest.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            validatedRequest.Scope = [ScopeConstants.OpenId];
        }

        return null;
    }

    /// <summary>
    /// ResponseTypes is OPTIONAL.
    /// Default is <see cref="ResponseTypeConstants.Code"/> if GrantTypes is <see cref="GrantTypeConstants.AuthorizationCode"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateResponseTypes(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (request.ResponseTypes.Count != 0
            && request.ResponseTypes.ExceptAny(DiscoveryDocument.ResponseTypesSupported))
        {
            return RegisterError.InvalidResponseTypes;
        }

        validatedRequest.ResponseTypes = request.ResponseTypes.Count == 0
                                         && validatedRequest.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode)
            ? [ResponseTypeConstants.Code]
            : request.ResponseTypes;

        return null;
    }

    /// <summary>
    /// RedirectUris is OPTIONAL.
    /// If GrantType is <see cref="GrantTypeConstants.AuthorizationCode"/> then it is REQUIRED.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateRedirectUris(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (request.RedirectUris.Count == 0
            && validatedRequest.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            return RegisterError.InvalidRedirectUris;
        }

        foreach (var redirectUri in request.RedirectUris)
        {
            if (validatedRequest.ApplicationType == ApplicationType.Native
                && !UrlHelper.IsUrlValidForNativeClient(redirectUri))
            {
                return RegisterError.InvalidRedirectUris;
            }

            if (validatedRequest.ApplicationType == ApplicationType.Web
                && !UrlHelper.IsUrlValidForWebClient(redirectUri))
            {
                return RegisterError.InvalidRedirectUris;
            }
        }

        validatedRequest.RedirectUris = request.RedirectUris;
        return null;
    }

    /// <summary>
    /// PostLogoutRedirectUris is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidatePostLogoutRedirectUris(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        foreach (var redirectUri in request.PostLogoutRedirectUris)
        {
            if (validatedRequest.ApplicationType == ApplicationType.Native
                && !UrlHelper.IsUrlValidForNativeClient(redirectUri))
            {
                return RegisterError.InvalidPostLogoutRedirectUris;
            }

            if (validatedRequest.ApplicationType == ApplicationType.Web
                && !UrlHelper.IsUrlValidForWebClient(redirectUri))
            {
                return RegisterError.InvalidPostLogoutRedirectUris;
            }
        }

        validatedRequest.PostLogoutRedirectUris = request.PostLogoutRedirectUris;
        return null;
    }

    /// <summary>
    /// RequestUris is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateRequestUris(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (validatedRequest.ApplicationType != ApplicationType.Web)
        {
            return null;
        }

        foreach (var requestUri in request.RequestUris)
        {
            if (!UrlHelper.IsUrlValidForWebClient(requestUri))
            {
                return RegisterError.InvalidRequestUris;
            }
        }

        validatedRequest.RequestUris = request.RequestUris;
        return null;
    }

    /// <summary>
    /// SectorIdentifierUri is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private async Task<ProcessError?> ValidateSectorIdentifierUri(RegisterRequest request, RegisterValidatedRequest validatedRequest, CancellationToken cancellationToken)
    {
        if (validatedRequest.SubjectType != SubjectType.Pairwise)
        {
            return null;
        }

        var hasSectorIdentifierUri = !string.IsNullOrEmpty(request.SectorIdentifierUri);
        var hasOneRedirectUri = validatedRequest.RedirectUris.Count == 1;

        if (!hasSectorIdentifierUri && !hasOneRedirectUri)
        {
            return RegisterError.InvalidSectorIdentifierUri;
        }

        if (!hasSectorIdentifierUri && hasOneRedirectUri)
        {
            validatedRequest.SectorIdentifierUri = validatedRequest.RedirectUris.Single();
            return null;
        }

        if (!UrlHelper.IsUrlValidForWebClient(request.SectorIdentifierUri!))
        {
            return RegisterError.InvalidSectorIdentifierUri;
        }

        var hasRedirectUrisInSectorDocument = await _clientSectorService.ContainsSectorDocument(
            new Uri(request.SectorIdentifierUri!), validatedRequest.RedirectUris, cancellationToken);

        if (!hasRedirectUrisInSectorDocument)
        {
            return RegisterError.InvalidSectorDocument;
        }

        validatedRequest.SectorIdentifierUri = request.SectorIdentifierUri;
        return null;
    }

    /// <summary>
    /// BackchannelLogoutUri is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateBackchannelLogoutUri(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.BackchannelLogoutUri))
        {
            return null;
        }

        if (!UrlHelper.IsUrlValidForWebClient(request.BackchannelLogoutUri))
        {
            return RegisterError.InvalidBackchannelLogoutUri;
        }

        validatedRequest.BackchannelLogoutUri = request.BackchannelLogoutUri;
        return null;
    }

    /// <summary>
    /// ClientUri is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateClientUri(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.ClientUri))
        {
            return null;
        }

        if (!UrlHelper.IsUrlValidForWebClient(request.ClientUri))
        {
            return RegisterError.InvalidClientUri;
        }

        validatedRequest.ClientUri = request.ClientUri;
        return null;
    }

    /// <summary>
    /// PolicyUri is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidatePolicyUri(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.PolicyUri))
        {
            return null;
        }

        if (!UrlHelper.IsUrlValidForWebClient(request.PolicyUri))
        {
            return RegisterError.InvalidPolicyUri;
        }

        validatedRequest.PolicyUri = request.PolicyUri;
        return null;
    }

    /// <summary>
    /// TosUri is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateTosUri(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.TosUri))
        {
            return null;
        }

        if (!UrlHelper.IsUrlValidForWebClient(request.TosUri))
        {
            return RegisterError.InvalidTosUri;
        }

        validatedRequest.TosUri = request.TosUri;
        return null;
    }

    /// <summary>
    /// InitiateLoginUri is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateInitiateLoginUri(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.InitiateLoginUri))
        {
            return null;
        }

        if (!UrlHelper.IsUrlValidForWebClient(request.InitiateLoginUri))
        {
            return RegisterError.InvalidInitiateLoginUri;
        }

        validatedRequest.InitiateLoginUri = request.InitiateLoginUri;
        return null;
    }

    /// <summary>
    /// LogoUri is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateLogoUri(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.LogoUri))
        {
            return null;
        }

        if (!UrlHelper.IsUrlValidForWebClient(request.LogoUri))
        {
            return RegisterError.InvalidLogoUri;
        }

        validatedRequest.LogoUri = request.LogoUri;
        return null;
    }

    /// <summary>
    /// Jwks is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateJwks(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.Jwks))
        {
            return null;
        }

        try
        {
            JsonWebKeySet.Create(request.Jwks);
            validatedRequest.Jwks = request.Jwks;
            return null;
        }
        catch (ArgumentException e)
        {
            _logger.LogInformation(e, "Jwks is invalid");
            return RegisterError.InvalidJwks;
        }
    }

    /// <summary>
    /// JwksUri is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<ProcessError?> ValidateJwksUri(RegisterRequest request,
        RegisterValidatedRequest validatedRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.JwksUri))
        {
            return null;
        }
        // TODO verify the application_type is web
        if (!UrlHelper.IsUrlValidForWebClient(request.JwksUri))
        {
            return RegisterError.InvalidJwksUri;
        }

        var jwksFromClient = await _clientJwkService.GetJwks(request.JwksUri, cancellationToken);
        if (string.IsNullOrEmpty(jwksFromClient))
        {
            return RegisterError.InvalidJwksUri;
        }

        try
        {
            JsonWebKeySet.Create(jwksFromClient);
            validatedRequest.Jwks = jwksFromClient;
            return null;
        }
        catch (ArgumentException e)
        {
            _logger.LogInformation(e, "JwksUri is invalid");
            return RegisterError.InvalidJwksUri;
        }
    }

    /// <summary>
    /// RequireSignedRequestObject is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateRequireSignedRequestObject(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (request.RequireSignedRequestObject is null)
        {
            return null;
        }

        validatedRequest.RequireSignedRequestObject = request.RequireSignedRequestObject.Value;
        return null;
    }

    /// <summary>
    /// RequireReferenceToken is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateRequireReferenceToken(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (request.RequireReferenceToken is null)
        {
            return null;
        }

        validatedRequest.RequireReferenceToken = request.RequireReferenceToken.Value;
        return null;
    }

    /// <summary>
    /// RequirePushedAuthorizationRequests is OPTIONAL.
    /// </summary>
    /// <returns></returns>
    private static ProcessError? ValidateRequirePushedAuthorizationRequests(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (request.RequirePushedAuthorizationRequests is null)
        {
            return null;
        }

        validatedRequest.RequirePushedAuthorizationRequests = request.RequirePushedAuthorizationRequests.Value;
        return null;
    }

    /// <summary>
    /// SubjectType is OPTIONAL.
    /// Default value is <see cref="SubjectType.Public"/> if GrantType is <see cref="GrantTypeConstants.AuthorizationCode"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateSubjectType(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.SubjectType))
        {
            validatedRequest.SubjectType = validatedRequest.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode)
                ? SubjectType.Public
                : null;

            return null;
        }

        if (!SubjectTypeConstants.SubjectTypes.Contains(request.SubjectType))
        {
            return RegisterError.InvalidSubjectType;
        }

        validatedRequest.SubjectType = request.SubjectType.GetEnum<SubjectType>();
        return null;
    }

    /// <summary>
    /// DefaultMaxAge is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateDefaultMaxAge(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.DefaultMaxAge))
        {
            return null;
        }

        if (MaxAgeHelper.IsMaxAgeValid(request.DefaultMaxAge))
        {
            return RegisterError.InvalidDefaultMaxAge;
        }

        validatedRequest.DefaultMaxAge = int.Parse(request.DefaultMaxAge);
        return null;
    }

    /// <summary>
    /// DefaultAcrValues is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateDefaultAcrValues(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (request.DefaultAcrValues.Count == 0)
        {
            return null;
        }

        if (request.DefaultAcrValues.ExceptAny(DiscoveryDocument.AcrValuesSupported))
        {
            return RegisterError.InvalidDefaultAcrValues;
        }

        validatedRequest.DefaultAcrValues = request.DefaultAcrValues;
        return null;
    }

    /// <summary>
    /// Contacts is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateContacts(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (request.Contacts.Count == 0)
        {
            return null;
        }

        var hasInvalidContacts = (from contact in request.Contacts
                let ampersandPosition = contact.LastIndexOf('@')
                select ampersandPosition > 0
                       && contact.LastIndexOf('.') > ampersandPosition
                       && contact.Length - ampersandPosition > 4)
            .Any(isValidContact => !isValidContact);

        if (hasInvalidContacts)
        {
            return RegisterError.InvalidContacts;
        }

        validatedRequest.Contacts = request.Contacts;
        return null;
    }

    /// <summary>
    /// AuthorizationCodeExpiration is OPTIONAL.
    /// Default is 60 if GrantType is <see cref="GrantTypeConstants.AuthorizationCode"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateAuthorizationCodeExpiration(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (request.AuthorizationCodeExpiration is null)
        {
            validatedRequest.AuthorizationCodeExpiration =
                validatedRequest.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode)
                    ? 60
                    : null;

            return null;
        }

        if (request.AuthorizationCodeExpiration is < 5 or > 600)
        {
            return RegisterError.InvalidAuthorizationCodeExpiration;
        }

        validatedRequest.AuthorizationCodeExpiration = request.AuthorizationCodeExpiration;
        return null;
    }

    /// <summary>
    /// AccessTokenExpiration is OPTIONAL.
    /// Default is 3600.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateAccessTokenExpiration(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (request.AccessTokenExpiration is null)
        {
            validatedRequest.AccessTokenExpiration = 3600;
            return null;
        }

        if (request.AccessTokenExpiration is < 60 or > 3600)
        {
            return RegisterError.InvalidAccessTokenExpiration;
        }

        validatedRequest.AccessTokenExpiration = request.AccessTokenExpiration.Value;
        return null;
    }

    /// <summary>
    /// RefreshTokenExpiration is OPTIONAL.
    /// Default value is 604800.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateRefreshTokenExpiration(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (request.RefreshTokenExpiration is null)
        {
            validatedRequest.RefreshTokenExpiration =
                validatedRequest.GrantTypes.Contains(GrantTypeConstants.RefreshToken)
                    ? 604800 // defaulted to 7 days
                    : null;

            return null;
        }

        // between 60 seconds and 60 days
        if (request.RefreshTokenExpiration is < 60 or > 5184000)
        {
            return RegisterError.InvalidRefreshTokenExpiration;
        }

        validatedRequest.RefreshTokenExpiration = request.RefreshTokenExpiration;
        return null;
    }

    /// <summary>
    /// ClientSecretExpiration is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateClientSecretExpiration(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (request.ClientSecretExpiration is null)
        {
            return null;
        }

        // not less than a day
        if (request.ClientSecretExpiration < 86400)
        {
            return RegisterError.InvalidClientSecretExpiration;
        }

        validatedRequest.ClientSecretExpiration = request.ClientSecretExpiration;
        return null;
    }

    /// <summary>
    /// JwksExpiration is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateJwksExpiration(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (request.JwksExpiration is null)
        {
            validatedRequest.JwksExpiration = !string.IsNullOrEmpty(validatedRequest.JwksUri)
                ? 86400
                : null;

            return null;
        }

        if (request.JwksExpiration < 0)
        {
            return RegisterError.InvalidJwksExpiration;
        }

        validatedRequest.JwksExpiration = request.JwksExpiration;
        return null;
    }

    /// <summary>
    /// RequestUriExpiration is OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private static ProcessError? ValidateRequestUriExpiration(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (request.RequestUriExpiration is null)
        {
            validatedRequest.RequestUriExpiration =
                validatedRequest.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode)
                    ? 300
                    : null;

            return null;
        }

        if (request.RequestUriExpiration is < 5 or > 600)
        {
            return RegisterError.InvalidRequestUriExpiration;
        }

        validatedRequest.RequestUriExpiration = request.RequestUriExpiration;
        return null;
    }

    /// <summary>
    /// TokenEndpointAuthSigningAlg is OPTIONAL.
    /// Default value is <see cref="SigningAlg.RsaSha256"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateTokenEndpointAuthSigningAlg(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.TokenEndpointAuthSigningAlg))
        {
            validatedRequest.TokenEndpointAuthSigningAlg = SigningAlg.RsaSha256;
            return null;
        }

        if (!DiscoveryDocument.TokenEndpointAuthSigningAlgValuesSupported.Contains(request.TokenEndpointAuthSigningAlg))
        {
            return RegisterError.InvalidTokenEndpointAuthSigningAlg;
        }

        validatedRequest.TokenEndpointAuthSigningAlg = request.TokenEndpointAuthSigningAlg.GetEnum<SigningAlg>();
        return null;
    }

    /// <summary>
    /// RequestObjectSigningAlg, RequestObjectEncryptionAlg and RequestObjectEncryptionEnc are OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateRequestObjectToken(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.RequestObjectSigningAlg))
        {
            return null;
        }

        if (!DiscoveryDocument.RequestObjectSigningAlgValuesSupported.Contains(request.RequestObjectSigningAlg))
        {
            return RegisterError.InvalidRequestObjectSigningAlg;
        }

        validatedRequest.RequestObjectSigningAlg = request.RequestObjectSigningAlg.GetEnum<SigningAlg>();

        var hasEmptyEncryptionAlg = string.IsNullOrEmpty(request.RequestObjectEncryptionAlg);
        var hasEmptyEncryptionEnc = string.IsNullOrEmpty(request.RequestObjectEncryptionEnc);

        if (hasEmptyEncryptionAlg && hasEmptyEncryptionEnc)
        {
            return null;
        }

        if (hasEmptyEncryptionAlg && !hasEmptyEncryptionEnc)
        {
            return RegisterError.InvalidRequestObjectEncryptionEnc;
        }

        if (!DiscoveryDocument.RequestObjectEncryptionAlgValuesSupported
                .Contains(request.RequestObjectEncryptionAlg!))
        {
            return RegisterError.InvalidRequestObjectEncryptionAlg;
        }

        if (!hasEmptyEncryptionEnc
            && !DiscoveryDocument.RequestObjectEncryptionEncValuesSupported
                .Contains(request.RequestObjectEncryptionEnc!))
        {
            return RegisterError.InvalidRequestObjectEncryptionEnc;
        }

        validatedRequest.RequestObjectEncryptionAlg = request.RequestObjectEncryptionAlg!.GetEnum<EncryptionAlg>();
        validatedRequest.RequestObjectEncryptionEnc = hasEmptyEncryptionEnc
            ? EncryptionEnc.Aes128CbcHmacSha256
            : request.RequestObjectEncryptionEnc!.GetEnum<EncryptionEnc>();

        return null;
    }

    /// <summary>
    /// UserinfoSignedResponseAlg, UserinfoEncryptedResponseAlg and UserinfoEncryptedResponseEnc are OPTIONAL. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateUserinfoResponseToken(RegisterRequest request,
        RegisterValidatedRequest validatedRequest)
    {
        if (string.IsNullOrEmpty(request.UserinfoSignedResponseAlg))
        {
            return null;
        }

        if (!DiscoveryDocument.UserinfoSigningAlgValuesSupported.Contains(request.UserinfoSignedResponseAlg))
        {
            return RegisterError.InvalidUserinfoSignedResponseAlg;
        }

        validatedRequest.UserinfoSignedResponseAlg = request.UserinfoSignedResponseAlg.GetEnum<SigningAlg>();

        var hasEmptyEncryptionAlg = string.IsNullOrEmpty(request.UserinfoEncryptedResponseAlg);
        var hasEmptyEncryptionEnc = string.IsNullOrEmpty(request.UserinfoEncryptedResponseEnc);

        if (hasEmptyEncryptionAlg && hasEmptyEncryptionEnc)
        {
            return null;
        }

        if (hasEmptyEncryptionAlg && !hasEmptyEncryptionEnc)
        {
            return RegisterError.InvalidRequestObjectEncryptionEnc;
        }

        if (!DiscoveryDocument.UserinfoEncryptionAlgValuesSupported.Contains(request.UserinfoEncryptedResponseAlg!))
        {
            return RegisterError.InvalidUserinfoEncryptedResponseAlg;
        }

        if (!hasEmptyEncryptionEnc &&
            !DiscoveryDocument.UserinfoEncryptionEncValuesSupported.Contains(request.UserinfoEncryptedResponseEnc!))
        {
            return RegisterError.InvalidUserinfoEncryptedResponseEnc;
        }

        validatedRequest.UserinfoEncryptedResponseAlg = request.UserinfoEncryptedResponseAlg!.GetEnum<EncryptionAlg>();
        validatedRequest.UserinfoEncryptedResponseEnc = request.UserinfoEncryptedResponseEnc!.GetEnum<EncryptionEnc>();
        return null;
    }

    /// <summary>
    /// IdTokenSignedResponseAlg, IdTokenEncryptedResponseAlg and IdTokenEncryptedResponseEnc are OPTIONAL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <returns></returns>
    private ProcessError? ValidateIdToken(RegisterRequest request, RegisterValidatedRequest validatedRequest)
    {
        var hasEmptyIdTokenSignedResponseAlg = string.IsNullOrEmpty(request.IdTokenSignedResponseAlg);
        if (hasEmptyIdTokenSignedResponseAlg &&
            !validatedRequest.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            return null;
        }

        if (hasEmptyIdTokenSignedResponseAlg)
        {
            validatedRequest.IdTokenSignedResponseAlg = SigningAlg.RsaSha256;
        }
        else if (!DiscoveryDocument.IdTokenSigningAlgValuesSupported.Contains(request.IdTokenSignedResponseAlg!))
        {
            return RegisterError.InvalidIdTokenSignedResponseAlg;
        }
        else
        {
            validatedRequest.IdTokenSignedResponseAlg = request.IdTokenSignedResponseAlg!.GetEnum<SigningAlg>();
        }

        var hasEmptyIdTokenEncryptedResponseAlg = string.IsNullOrEmpty(request.IdTokenEncryptedResponseAlg);
        var hasEmptyIdTokenEncryptedResponseEnc = string.IsNullOrEmpty(request.IdTokenEncryptedResponseEnc);

        if (hasEmptyIdTokenEncryptedResponseAlg && hasEmptyIdTokenEncryptedResponseEnc)
        {
            return null;
        }

        if (hasEmptyIdTokenEncryptedResponseAlg && !hasEmptyIdTokenEncryptedResponseEnc)
        {
            return RegisterError.InvalidIdTokenEncryptedResponseEnc;
        }

        if (!DiscoveryDocument.IdTokenEncryptionAlgValuesSupported.Contains(request.IdTokenEncryptedResponseAlg!))
        {
            return RegisterError.InvalidIdTokenEncryptedResponseAlg;
        }

        if (!hasEmptyIdTokenEncryptedResponseEnc &&
            !DiscoveryDocument.IdTokenEncryptionEncValuesSupported.Contains(request.IdTokenEncryptedResponseEnc!))
        {
            return RegisterError.InvalidIdTokenEncryptedResponseEnc;
        }

        validatedRequest.IdTokenEncryptedResponseAlg = request.IdTokenSignedResponseAlg!.GetEnum<EncryptionAlg>();
        validatedRequest.IdTokenEncryptedResponseEnc = hasEmptyIdTokenEncryptedResponseEnc
            ? EncryptionEnc.Aes128CbcHmacSha256
            : request.IdTokenEncryptedResponseEnc!.GetEnum<EncryptionEnc>();
        return null;
    }

    /// <summary>
    /// ClientId and AccessToken are REQUIRED for management endpoints.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validatedRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<ProcessError?> ValidateManagementParameters(RegisterRequest request,
        RegisterValidatedRequest validatedRequest,
        CancellationToken cancellationToken)
    {
        // POST is not management
        if (request.Method == HttpMethod.Post)
        {
            return null;
        }

        /* ClientId is REQUIRED */
        var clientId = request.ClientId;
        if (string.IsNullOrEmpty(clientId))
        {
            return RegisterError.InvalidClientId;
        }

        /* RegistrationAccessToken is REQUIRED */
        var registrationAccessToken = request.RegistrationAccessToken;
        if (string.IsNullOrEmpty(registrationAccessToken))
        {
            return RegisterError.InvalidRegistrationAccessToken;
        }

        var token = await _tokenRepository.GetActiveRegistrationToken(registrationAccessToken, cancellationToken);
        if (token is null)
        {
            return RegisterError.InvalidRegistrationAccessToken;
        }

        if (token.Client.Id != clientId)
        {
            return RegisterError.MismatchingClientId;
        }

        validatedRequest.ClientId = clientId;
        validatedRequest.RegistrationAccessToken = registrationAccessToken;
        return null;
    }
}