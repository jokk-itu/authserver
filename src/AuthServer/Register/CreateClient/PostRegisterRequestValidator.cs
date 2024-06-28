using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Register;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Register.CreateClient;

internal class PostRegisterRequestValidator : IRequestValidator<PostRegisterRequest, PostRegisterValidatedRequest>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly IClientJwkService _clientJwksService;
    private readonly ILogger<PostRegisterRequestValidator> _logger;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    private DiscoveryDocument DiscoveryDocument => _discoveryDocumentOptions.Value;

    public PostRegisterRequestValidator(
        AuthorizationDbContext authorizationDbContext,
        IClientJwkService clientJwksService,
        ILogger<PostRegisterRequestValidator> logger,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _authorizationDbContext = authorizationDbContext;
        _clientJwksService = clientJwksService;
        _logger = logger;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public async Task<ProcessResult<PostRegisterValidatedRequest, ProcessError>> Validate(PostRegisterRequest request,
        CancellationToken cancellationToken)
    {
        /* ApplicationType is OPTIONAL. If not provided, it is set to web. */
        if (!string.IsNullOrEmpty(request.ApplicationType)
            && !ApplicationTypeConstants.ApplicationTypes.Contains(request.ApplicationType))
        {
            return RegisterError.InvalidApplicationType;
        }

        var applicationType = string.IsNullOrEmpty(request.ApplicationType)
            ? ApplicationType.Web
            : request.ApplicationType.GetEnum<ApplicationType>();


        /* TokenEndpointAuthMethod is OPTIONAL. If not provided, it is set to client_secret_basic. */
        if (!string.IsNullOrEmpty(request.TokenEndpointAuthMethod)
            && !DiscoveryDocument.TokenEndpointAuthMethodsSupported.Contains(request.TokenEndpointAuthMethod))
        {
            // TODO invalid TokenEndpointAuthMethod given
        }
        // TODO validate if PrivateKey, then jwks or jwks_uri must be set

        var tokenEndpointAuthMethod = string.IsNullOrEmpty(request.TokenEndpointAuthMethod)
            ? TokenEndpointAuthMethod.ClientSecretBasic
            : request.TokenEndpointAuthMethod.GetEnum<TokenEndpointAuthMethod>();


        /* ClientName is REQUIRED and not duplicate. */
        if (string.IsNullOrEmpty(request.ClientName))
        {
            // TODO ClientName is required
        }

        var clientNameIsDuplicate = await _authorizationDbContext
            .Set<Client>()
            .AnyAsync(x => x.Name == request.ClientName, cancellationToken);

        if (clientNameIsDuplicate)
        {
            // TODO ClientName is duplicate
        }


        /* GrantTypes is OPTIONAL. If not provided, it is set to AuthorizationCode. */
        if (request.GrantTypes.Count != 0
            && request.GrantTypes.Except(DiscoveryDocument.GrantTypesSupported).Any())
        {
            // TODO invalid GrantType given
        }

        var grantTypes = request.GrantTypes.Count == 0
            ? [GrantTypeConstants.AuthorizationCode]
            : request.GrantTypes;


        /* Scope is OPTIONAL. If not provided, it is set to OpenId if OIDC applicable GrantType is requested. */
        var scope = request.Scope;
        if (scope.Count != 0)
        {
            var existingScope = DiscoveryDocument.ScopesSupported.Count;
            if (existingScope != scope.Count)
            {
                // TODO invalid Scope given
            }
        }
        else if (grantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            scope = [ScopeConstants.OpenId];
        }


        /* ResponseTypes is OPTIONAL. If not provided, it is set to code if OIDC applicable GranType is requested. */
        if (request.ResponseTypes.Count != 0
            && DiscoveryDocument.ResponseTypesSupported.Except(request.ResponseTypes).Any())
        {
            // TODO invalid ResponseType given
        }

        var responseTypes = request.ResponseTypes.Count == 0
                            && grantTypes.Contains(GrantTypeConstants.AuthorizationCode)
            ? [ResponseTypeConstants.Code]
            : request.ResponseTypes;


        /* RedirectUris is REQUIRED if OIDC applicable GrantType is requested, otherwise it is OPTIONAL. */
        var redirectUris = request.RedirectUris;
        var hasEmptyRedirectUris = redirectUris.Count == 0;
        if (hasEmptyRedirectUris
            && grantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            // TODO RedirectUris is required
        }

        if (!hasEmptyRedirectUris && applicationType == ApplicationType.Native)
        {
            foreach (var redirectUri in redirectUris)
            {
                var isValidUri = Uri.TryCreate(redirectUri, UriKind.Absolute, out var parsedRedirectUri);
                if (!isValidUri)
                {
                    // TODO invalid RedirectUris given
                }
                else if (!string.IsNullOrEmpty(parsedRedirectUri!.Fragment))
                {
                    // TODO invalid RedirectUris given
                }

                var isHttps = parsedRedirectUri!.Scheme == Uri.UriSchemeHttps;
                var isLoopback = parsedRedirectUri!.IsLoopback;
                var isPrivateScheme = parsedRedirectUri!.Scheme.Split('.').Length > 1;
                if (!isHttps && !isLoopback && !isPrivateScheme)
                {
                    // TODO invalid RedirectUris given
                }
            }
        }
        else if (!hasEmptyRedirectUris && applicationType == ApplicationType.Web)
        {
            foreach (var redirectUri in redirectUris)
            {
                var isValidUri = Uri.TryCreate(redirectUri, UriKind.Absolute, out var parsedRedirectUri);
                if (!isValidUri)
                {
                    // TODO invalid RedirectUris given
                }
                else if (parsedRedirectUri!.Scheme != Uri.UriSchemeHttps)
                {
                    // TODO invalid RedirectUris given
                }
                else if (parsedRedirectUri!.IsLoopback)
                {
                    // TODO invalid RedirectUris given
                }
                else if (!string.IsNullOrEmpty(parsedRedirectUri!.Fragment))
                {
                    // TODO invalid RedirectUris given
                }
            }
        }


        /* PostLogoutRedirectUris is OPTIONAL. */
        var postLogoutRedirectUris = request.PostLogoutRedirectUris;
        var hasEmptyPostLogoutRedirectUris = postLogoutRedirectUris.Count == 0;
        if (!hasEmptyPostLogoutRedirectUris && applicationType == ApplicationType.Native)
        {
            foreach (var redirectUri in redirectUris)
            {
                var isValidUri = Uri.TryCreate(redirectUri, UriKind.Absolute, out var parsedRedirectUri);
                if (!isValidUri)
                {
                    // TODO invalid PostLogoutRedirectUris given
                }
                else if (!string.IsNullOrEmpty(parsedRedirectUri!.Fragment))
                {
                    // TODO invalid PostLogoutRedirectUris given
                }

                var isHttps = parsedRedirectUri!.Scheme == Uri.UriSchemeHttps;
                var isLoopback = parsedRedirectUri!.IsLoopback;
                var isPrivateScheme = parsedRedirectUri!.Scheme.Split('.').Length > 1;
                if (!isHttps && !isLoopback && !isPrivateScheme)
                {
                    // TODO invalid PostLogoutRedirectUris given
                }
            }
        }
        else if (!hasEmptyPostLogoutRedirectUris && applicationType == ApplicationType.Web)
        {
            foreach (var redirectUri in redirectUris)
            {
                var isValidUri = Uri.TryCreate(redirectUri, UriKind.Absolute, out var parsedRedirectUri);
                if (!isValidUri)
                {
                    // TODO invalid PostLogoutRedirectUris given
                }
                else if (parsedRedirectUri!.Scheme != Uri.UriSchemeHttps)
                {
                    // TODO invalid PostLogoutRedirectUris given
                }
                else if (parsedRedirectUri!.IsLoopback)
                {
                    // TODO invalid PostLogoutRedirectUris given
                }
                else if (!string.IsNullOrEmpty(parsedRedirectUri!.Fragment))
                {
                    // TODO invalid PostLogoutRedirectUris given
                }
            }
        }


        /* RequestUris is OPTIONAL. */
        var requestUris = request.RequestUris;
        var hasEmptyRequestUris = requestUris.Count == 0;
        if (!hasEmptyRequestUris)
        {
            foreach (var requestUri in requestUris)
            {
                var isValidUri = Uri.TryCreate(requestUri, UriKind.Absolute, out var parsedRequestUri);
                if (!isValidUri)
                {
                    // TODO invalid RequestUris given
                }
                else if (parsedRequestUri!.IsLoopback)
                {
                    // TODO invalid RequestUris given
                }
                else if (parsedRequestUri!.Scheme != Uri.UriSchemeHttps)
                {
                    // TODO invalid RequestUris given
                }
            }
        }


        /* BackchannelLogoutUri is OPTIONAL. */
        var backchannelLogoutUri = request.BackchannelLogoutUri;
        var hasEmptyBackchannelLogoutUri = string.IsNullOrEmpty(backchannelLogoutUri);
        if (!hasEmptyBackchannelLogoutUri)
        {
            var isValidUri = Uri.TryCreate(backchannelLogoutUri, UriKind.Absolute, out var parsedBackchannelLogoutUri);
            if (!isValidUri)
            {
                // TODO invalid BackchannelLogoutUri given
            }
            else if (parsedBackchannelLogoutUri!.IsLoopback)
            {
                // TODO invalid BackchannelLogoutUri given
            }
            else if (parsedBackchannelLogoutUri!.Scheme != Uri.UriSchemeHttps)
            {
                // TODO invalid BackchannelLogoutUri given
            }
        }


        /* ClientUri is OPTIONAL. */
        var clientUri = request.ClientUri;
        var hasEmptyClientUri = string.IsNullOrEmpty(clientUri);
        if (!hasEmptyClientUri)
        {
            var isValidUri = Uri.TryCreate(clientUri, UriKind.Absolute, out var parsedClientUri);
            if (!isValidUri)
            {
                // TODO invalid ClientUri given
            }
            else if (parsedClientUri!.IsLoopback)
            {
                // TODO invalid ClientUri given
            }
            else if (parsedClientUri!.Scheme != Uri.UriSchemeHttps || parsedClientUri!.Scheme != Uri.UriSchemeHttp)
            {
                // TODO invalid ClientUri given
            }
        }


        /* PolicyUri is OPTIONAL. */
        var policyUri = request.PolicyUri;
        var hasEmptyPolicyUri = string.IsNullOrEmpty(policyUri);
        if (!hasEmptyPolicyUri)
        {
            var isValidUri = Uri.TryCreate(policyUri, UriKind.Absolute, out var parsedPolicyUri);
            if (!isValidUri)
            {
                // TODO invalid PolicyUri given
            }
            else if (parsedPolicyUri!.IsLoopback)
            {
                // TODO invalid PolicyUri given
            }
            else if (parsedPolicyUri!.Scheme != Uri.UriSchemeHttps || parsedPolicyUri!.Scheme != Uri.UriSchemeHttp)
            {
                // TODO invalid PolicyUri given
            }
        }


        /* TosUri is OPTIONAL. */
        var tosUri = request.TosUri;
        var hasEmptyTosUri = string.IsNullOrEmpty(tosUri);
        if (!hasEmptyTosUri)
        {
            var isValidUri = Uri.TryCreate(tosUri, UriKind.Absolute, out var parsedTosUri);
            if (!isValidUri)
            {
                // TODO invalid TosUri given
            }
            else if (parsedTosUri!.IsLoopback)
            {
                // TODO invalid TosUri given
            }
            else if (parsedTosUri!.Scheme != Uri.UriSchemeHttps || parsedTosUri!.Scheme != Uri.UriSchemeHttp)
            {
                // TODO invalid TosUri given
            }
        }


        /* InitiateLoginUri is OPTIONAL. */
        var initiateLoginUri = request.InitiateLoginUri;
        var hasEmptyInitiateLoginUri = string.IsNullOrEmpty(initiateLoginUri);
        if (!hasEmptyInitiateLoginUri)
        {
            var isValidUri = Uri.TryCreate(initiateLoginUri, UriKind.Absolute, out var parsedInitiateLoginUri);
            if (!isValidUri)
            {
                // TODO invalid InitiateLoginUri given
            }
            else if (parsedInitiateLoginUri!.IsLoopback)
            {
                // TODO invalid InitiateLoginUri given
            }
            else if (parsedInitiateLoginUri!.Scheme != Uri.UriSchemeHttps ||
                     parsedInitiateLoginUri!.Scheme != Uri.UriSchemeHttp)
            {
                // TODO invalid InitiateLoginUri given
            }
        }


        /* LogoUri is OPTIONAL. */
        var logoUri = request.LogoUri;
        var hasEmptyLogoUri = string.IsNullOrEmpty(logoUri);
        if (!hasEmptyLogoUri)
        {
            var isValidUri = Uri.TryCreate(logoUri, UriKind.Absolute, out var parsedLogoUri);
            if (!isValidUri)
            {
                // TODO invalid LogoUri given
            }
            else if (parsedLogoUri!.IsLoopback)
            {
                // TODO invalid LogoUri given
            }
            else if (parsedLogoUri!.Scheme != Uri.UriSchemeHttps || parsedLogoUri!.Scheme != Uri.UriSchemeHttp)
            {
                // TODO invalid LogoUri given
            }
        }


        /* Jwks and Jwks_Uri is OPTIONAL. */
        var jwks = request.Jwks;
        var jwksUri = request.JwksUri;
        var hasBothJwkParametersSet = !string.IsNullOrEmpty(request.Jwks)
                                      && !string.IsNullOrEmpty(request.JwksUri);
        if (hasBothJwkParametersSet)
        {
            // TODO invalid Jwks and Jwks_Uri given
        }
        else if (!string.IsNullOrEmpty(request.Jwks))
        {
            try
            {
                JsonWebKeySet.Create(request.Jwks);
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Unexpected error occurred deserializing jwks");
                // TODO invalid JwksUri given
            }
        }
        else if (!string.IsNullOrEmpty(request.JwksUri))
        {
            var isValidUri = Uri.TryCreate(request.JwksUri, UriKind.Absolute, out var parsedJwksUri);
            if (!isValidUri)
            {
                // TODO invalid JwksUri given
            }
            else if (parsedJwksUri!.IsLoopback)
            {
                // TODO invalid JwksUri given
            }
            else if (parsedJwksUri!.Scheme != Uri.UriSchemeHttps)
            {
                // TODO invalid JwksUri given
            }

            var jwksFromClient = await _clientJwksService.GetJwks(request.JwksUri, cancellationToken);
            if (string.IsNullOrEmpty(jwksFromClient))
            {
                // TODO invalid JwksUri given
            }

            try
            {
                JsonWebKeySet.Create(jwksFromClient);
                jwks = jwksFromClient;
            }
            catch (ArgumentException e)
            {
                _logger.LogError(e, "Unexpected error occurred deserializing jwks");
                // TODO invalid JwksUri given
            }
        }


        /* RequireSignedRequestObject is OPTIONAL. */
        var requireSignedRequestObject = false;
        if (!string.IsNullOrEmpty(request.RequireSignedRequestObject))
        {
            if (!bool.TryParse(request.RequireSignedRequestObject, out var parsedRequireSignedRequestObject))
            {
                // TODO invalid RequireSignedRequestObject given
            }

            requireSignedRequestObject = parsedRequireSignedRequestObject;
        }

        /* RequireReferenceToken is OPTIONAL */
        var requireReferenceToken = false;
        if (!string.IsNullOrEmpty(request.RequireReferenceToken))
        {
            if (!bool.TryParse(request.RequireReferenceToken, out var parsedRequireReferenceToken))
            {
                // TODO invalid RequireReferenceToken given
            }

            requireReferenceToken = parsedRequireReferenceToken;
        }


        /* SubjectType is OPTIONAL. If not provided, it is set to Public if OIDC applicable GrantType is requested. */
        if (!string.IsNullOrEmpty(request.SubjectType)
            && !SubjectTypeConstants.SubjectTypes.Contains(request.SubjectType))
        {
            // TODO invalid SubjectType given
        }

        var subjectType = string.IsNullOrEmpty(request.SubjectType)
                          && grantTypes.Contains(GrantTypeConstants.AuthorizationCode)
            ? SubjectType.Public as SubjectType?
            : null;


        /* DefaultMaxAge is OPTIONAL. */
        int? defaultMaxAge = null;
        var isEmptyDefaultMaxAge = string.IsNullOrEmpty(request.DefaultMaxAge);
        if (!isEmptyDefaultMaxAge)
        {
            var isValidMaxAge = int.TryParse(request.DefaultMaxAge, out var parsedMaxAge);
            if (!isValidMaxAge)
            {
                // TODO invalid DefaultMaxAge given
            }
            else if (parsedMaxAge < 0)
            {
                // TODO invalid DefaultMaxAge given
            }

            defaultMaxAge = parsedMaxAge;
        }


        /* DefaultAcrValues is OPTIONAL. */
        var defaultAcrValues = request.DefaultAcrValues;
        var hasEmptyDefaultAcrValues = defaultAcrValues.Count == 0;
        if (!hasEmptyDefaultAcrValues && DiscoveryDocument.AcrValuesSupported.Except(defaultAcrValues).Any())
        {
            // TODO invalid DefaultAcrValues given
        }


        /* Contacts is OPTIONAL. */
        var contacts = request.Contacts;
        var hasEmptyContacts = contacts.Count == 0;
        if (!hasEmptyContacts)
        {
            var hasInvalidContacts = (from contact in contacts
                                      let ampersandPosition = contact.LastIndexOf('@')
                                      select ampersandPosition > 0
                                             && contact.LastIndexOf('.') > ampersandPosition
                                             && contact.Length - ampersandPosition > 4)
                .Any(isValidContact => !isValidContact);

            if (hasInvalidContacts)
            {
                // TODO invalid Contacts given
            }
        }


        /* AuthorizationCodeExpiration is OPTIONAL. */
        int? authorizationCodeExpiration = null;
        var hasEmptyAuthorizationCodeExpiration = string.IsNullOrEmpty(request.AuthorizationCodeExpiration);
        if (!hasEmptyAuthorizationCodeExpiration)
        {
            if (!grantTypes.Contains(GrantTypeConstants.AuthorizationCode))
            {
                // TODO invalid
            }

            var isValidExpiration = int.TryParse(request.AuthorizationCodeExpiration,
                out var parsedAuthorizationCodeExpiration);
            if (!isValidExpiration)
            {
                // TODO invalid AuthorizationCodeExpiration given
            }
            // between 5 seconds and 10 minutes
            else if (parsedAuthorizationCodeExpiration is < 5 or > 600)
            {
                // TODO invalid AuthorizationCodeExpiration given
            }

            authorizationCodeExpiration = parsedAuthorizationCodeExpiration;
        }
        else if (grantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            // default is 1 minute
            authorizationCodeExpiration = 60;
        }


        /* AccessTokenExpiration is OPTIONAL. */
        var accessTokenExpiration = 3600;
        var hasEmptyAccessTokenExpiration = string.IsNullOrEmpty(request.AccessTokenExpiration);
        if (!hasEmptyAccessTokenExpiration)
        {
            var isValidExpiration = int.TryParse(request.AccessTokenExpiration, out var parsedAccessTokenExpiration);
            if (!isValidExpiration)
            {
                // TODO invalid AccessTokenExpiration given
            }
            // between 1 minute and 1 hour
            else if (parsedAccessTokenExpiration is < 60 or > 3600)
            {
                // TODO invalid AccessTokenExpiration given
            }

            accessTokenExpiration = parsedAccessTokenExpiration;
        }


        /* RefreshTokenExpiration is OPTIONAL. */
        int? refreshTokenExpiration = null;
        var hasEmptyRefreshTokenExpiration = string.IsNullOrEmpty(request.RefreshTokenExpiration);
        if (!hasEmptyRefreshTokenExpiration)
        {
            if (!grantTypes.Contains(GrantTypeConstants.RefreshToken))
            {
                // TODO invalid
            }

            var isValidExpiration = int.TryParse(request.RefreshTokenExpiration, out var parsedRefreshTokenExpiration);
            if (!isValidExpiration)
            {
                // TODO invalid RefreshTokenExpiration given
            }
            // between 1 minute or 60 days
            else if (parsedRefreshTokenExpiration is < 60 or > 5184000)
            {
                // TODO invalid RefreshTokenExpiration given
            }

            refreshTokenExpiration = parsedRefreshTokenExpiration;
        }
        else if (grantTypes.Contains(GrantTypeConstants.RefreshToken))
        {
            // default is 7 days
            refreshTokenExpiration = 604800;
        }


        /* ClientSecret is OPTIONAL. */
        int? clientSecretExpiration = null;
        var hasEmptyClientSecretExpiration = string.IsNullOrEmpty(request.ClientSecretExpiration);
        if (!hasEmptyClientSecretExpiration)
        {
            if (tokenEndpointAuthMethod == TokenEndpointAuthMethod.None)
            {
                // TODO invalid
            }

            var isValidExpiration = int.TryParse(request.ClientSecretExpiration, out var parsedClientSecretExpiration);
            if (!isValidExpiration)
            {
                // TODO invalid ClientSecretExpiration given
            }
            // anything less than a day is useless
            else if (parsedClientSecretExpiration < 86400)
            {
                // TODO invalid ClientSecretExpiration given
            }

            clientSecretExpiration = parsedClientSecretExpiration;
        }


        /* JwksExpiration */
        int? jwksExpiration = null;
        var hasEmptyJwksExpiration = string.IsNullOrEmpty(request.JwksExpiration);
        if (!hasEmptyJwksExpiration)
        {
            // validates static and dynamic jwks
            if (string.IsNullOrEmpty(jwks))
            {
                // TODO invalid JwksExpiration
            }

            var isValidExpiration = int.TryParse(request.JwksExpiration, out var parsedJwksExpiration);
            if (!isValidExpiration)
            {
                // TODO invalid JwksExpiration given
            }
            else if (parsedJwksExpiration < 0)
            {
                // TODO invalid JwksExpiration given
            }

            jwksExpiration = parsedJwksExpiration;
        }
        else if (!string.IsNullOrEmpty(jwksUri))
        {
            // default to one day, but only for reference jwks
            jwksExpiration = 86400;
        }


        /* TokenEndpointAuthSigningAlg is OPTIONAL. If not provided, it is set to RS256. */
        var tokenEndpointAuthSigningAlg = SigningAlg.RsaSha256;
        var hasEmptyTokenEndpointAuthSigningAlg = string.IsNullOrEmpty(request.TokenEndpointAuthSigningAlg);
        if (!hasEmptyTokenEndpointAuthSigningAlg)
        {
            if (!DiscoveryDocument.TokenEndpointAuthSigningAlgValuesSupported.Contains(request
                    .TokenEndpointAuthSigningAlg))
            {
                // TODO invalid TokenEndpointAuthSigningAlg
            }

            tokenEndpointAuthSigningAlg = request.TokenEndpointAuthSigningAlg.GetEnum<SigningAlg>();
        }


        /* RequestObjectSigningAlg is OPTIONAL. */
        SigningAlg? requestObjectSigningAlg = null;
        var hasEmptyRequestObjectSigningAlg = string.IsNullOrEmpty(request.RequestObjectSigningAlg);

        /* RequestObjectEncryptionAlg is OPTIONAL. */
        EncryptionAlg? requestObjectEncryptionAlg = null;
        var hasEmptyRequestObjectEncryptionAlg = string.IsNullOrEmpty(request.RequestObjectEncryptionAlg);

        /* RequestObjectEncryptionEnc is OPTIONAL. */
        EncryptionEnc? requestObjectEncryptionEnc = null;
        var hasEmptyRequestObjectEncryptionEnc = string.IsNullOrEmpty(request.RequestObjectEncryptionEnc);

        if (!hasEmptyRequestObjectSigningAlg)
        {
            if (!DiscoveryDocument.RequestObjectSigningAlgValuesSupported.Contains(request.RequestObjectSigningAlg))
            {
                // TODO invalid RequestObjectSigningAlg
            }

            requestObjectSigningAlg = request.RequestObjectSigningAlg.GetEnum<SigningAlg>();

            if (hasEmptyRequestObjectEncryptionAlg && !hasEmptyRequestObjectEncryptionEnc)
            {
                // TODO Alg must be set if Enc is set
            }
            else if (!hasEmptyRequestObjectEncryptionAlg &&
                     !DiscoveryDocument.RequestObjectEncryptionAlgValuesSupported.Contains(
                         request.RequestObjectEncryptionAlg))
            {
                // TODO invalid RequestObjectEncryptionAlg
            }
            else if (!hasEmptyRequestObjectEncryptionEnc &&
                     !DiscoveryDocument.RequestObjectEncryptionEncValuesSupported.Contains(
                         request.RequestObjectEncryptionEnc))
            {
                // TODO invalid RequestObjectEncryptionEnc
            }
            else if (!hasEmptyRequestObjectEncryptionAlg && !hasEmptyRequestObjectEncryptionEnc)
            {
                requestObjectEncryptionAlg = request.RequestObjectEncryptionAlg.GetEnum<EncryptionAlg>();
                requestObjectEncryptionEnc = hasEmptyRequestObjectEncryptionEnc
                    ? EncryptionEnc.Aes128CbcHmacSha256
                    : request.RequestObjectEncryptionEnc.GetEnum<EncryptionEnc>();
            }
        }


        /* UserinfoSignedResponseAlg is OPTIONAL. */
        SigningAlg? userinfoSignedResponseAlg = null;
        var hasEmptyUserinfoSignedResponseAlg = string.IsNullOrEmpty(request.UserinfoSignedResponseAlg);

        /* UserinfoEncryptedResponseAlg is OPTIONAL. */
        EncryptionAlg? userinfoEncryptedResponseAlg = null;
        var hasEmptyUserinfoEncryptedResponseAlg = string.IsNullOrEmpty(request.UserinfoEncryptedResponseAlg);

        /* UserinfoEncryptedResponseEnc is OPTIONAL. */
        EncryptionEnc? userinfoEncryptedResponseEnc = null;
        var hasEmptyUserinfoEncryptedResponseEnc = string.IsNullOrEmpty(request.UserinfoEncryptedResponseEnc);

        if (!hasEmptyUserinfoSignedResponseAlg)
        {
            if (!DiscoveryDocument.UserinfoSigningAlgValuesSupported.Contains(request.UserinfoSignedResponseAlg))
            {
                // TODO invalid UserinfoSignedResponseAlg
            }

            userinfoSignedResponseAlg = request.UserinfoSignedResponseAlg.GetEnum<SigningAlg>();

            if (hasEmptyUserinfoEncryptedResponseAlg && !hasEmptyUserinfoEncryptedResponseEnc)
            {
                // TODO Alg must be set if Enc is set
            }
            else if (!hasEmptyUserinfoEncryptedResponseAlg &&
                     !DiscoveryDocument.UserinfoEncryptionAlgValuesSupported.Contains(
                         request.UserinfoEncryptedResponseAlg))
            {
                // TODO invalid UserinfoEncryptedResponseAlg
            }
            else if (!hasEmptyUserinfoEncryptedResponseEnc &&
                     !DiscoveryDocument.UserinfoEncryptionEncValuesSupported.Contains(
                         request.UserinfoEncryptedResponseEnc))
            {
                // TODO invalid UserinfoEncryptedResponseEnc
            }
            else if (!hasEmptyUserinfoEncryptedResponseAlg && !hasEmptyUserinfoEncryptedResponseEnc)
            {
                userinfoEncryptedResponseAlg = request.UserinfoEncryptedResponseAlg.GetEnum<EncryptionAlg>();
                userinfoEncryptedResponseEnc = hasEmptyUserinfoEncryptedResponseEnc
                    ? EncryptionEnc.Aes128CbcHmacSha256
                    : request.UserinfoEncryptedResponseEnc.GetEnum<EncryptionEnc>();
            }
        }


        /* IdTokenSignedResponseAlg is OPTIONAL. */
        SigningAlg? idTokenSignedResponseAlg = null;
        var hasEmptyIdTokenSignedResponseAlg = string.IsNullOrEmpty(request.IdTokenSignedResponseAlg);

        /* IdTokenEncryptedResponseAlg is OPTIONAL. */
        EncryptionAlg? idTokenEncryptedResponseAlg = null;
        var hasEmptyIdTokenEncryptedResponseAlg = string.IsNullOrEmpty(request.IdTokenEncryptedResponseAlg);

        /* IdTokenEncryptedResponseEnc is OPTIONAL. */
        EncryptionEnc? idTokenEncryptedResponseEnc = null;
        var hasEmptyIdTokenEncryptedResponseEnc = string.IsNullOrEmpty(request.IdTokenEncryptedResponseEnc);

        if (!hasEmptyIdTokenEncryptedResponseAlg || grantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            if (hasEmptyIdTokenSignedResponseAlg)
            {
                idTokenSignedResponseAlg = SigningAlg.RsaSha256;
            }
            else if (!DiscoveryDocument.IdTokenSigningAlgValuesSupported.Contains(request.IdTokenSignedResponseAlg))
            {
                // TODO invalid IdTokenSignedResponseAlg
            }
            else
            {
                idTokenSignedResponseAlg = request.IdTokenSignedResponseAlg.GetEnum<SigningAlg>();
            }

            if (hasEmptyIdTokenEncryptedResponseAlg && !hasEmptyIdTokenEncryptedResponseEnc)
            {
                // TODO Alg must be set if Enc is set
            }
            else if (!hasEmptyIdTokenEncryptedResponseAlg &&
                     !DiscoveryDocument.IdTokenEncryptionAlgValuesSupported.Contains(
                         request.IdTokenEncryptedResponseAlg))
            {
                // TODO invalid IdTokenEncryptedResponseAlg
            }
            else if (!hasEmptyIdTokenEncryptedResponseEnc &&
                     !DiscoveryDocument.IdTokenEncryptionEncValuesSupported.Contains(
                         request.IdTokenEncryptedResponseEnc))
            {
                // TODO invalid IdTokenEncryptedResponseEnc
            }
            else if (!hasEmptyIdTokenEncryptedResponseAlg && !hasEmptyIdTokenEncryptedResponseEnc)
            {
                idTokenEncryptedResponseAlg = request.IdTokenEncryptedResponseAlg.GetEnum<EncryptionAlg>();
                idTokenEncryptedResponseEnc = request.IdTokenEncryptedResponseEnc.GetEnum<EncryptionEnc>();
            }
        }


        /* AuthorizationSignedResponseAlg is OPTIONAL. */
        SigningAlg? authorizationSignedResponseAlg = null;
        var hasEmptyAuthorizationSignedResponseAlg = string.IsNullOrEmpty(request.AuthorizationSignedResponseAlg);

        /* AuthorizationEncryptedResponseAlg is OPTIONAL. */
        EncryptionAlg? authorizationEncryptedResponseAlg = null;
        var hasEmptyAuthorizationEncryptedResponseAlg = string.IsNullOrEmpty(request.AuthorizationEncryptedResponseAlg);

        /* AuthorizationEncryptedResponseEnc is OPTIONAL. */
        EncryptionEnc? authorizationEncryptedResponseEnc = null;
        var hasEmptyAuthorizationEncryptedResponseEnc = string.IsNullOrEmpty(request.AuthorizationEncryptedResponseEnc);

        if (!hasEmptyAuthorizationSignedResponseAlg || grantTypes.Contains(GrantTypeConstants.AuthorizationCode))
        {
            if (hasEmptyAuthorizationSignedResponseAlg)
            {
                authorizationSignedResponseAlg = SigningAlg.RsaSha256;
            }
            else if (!DiscoveryDocument.AuthorizationSigningAlgValuesSupported.Contains(request
                         .AuthorizationSignedResponseAlg))
            {
                // TODO invalid AuthorizationSignedResponseAlg
            }
            else
            {
                authorizationSignedResponseAlg = request.AuthorizationSignedResponseAlg.GetEnum<SigningAlg>();
            }

            if (hasEmptyAuthorizationEncryptedResponseAlg && !hasEmptyAuthorizationEncryptedResponseEnc)
            {
                // TODO Alg must be set if Enc is set
            }
            else if (!hasEmptyAuthorizationEncryptedResponseAlg &&
                     !DiscoveryDocument.AuthorizationEncryptionAlgValuesSupported.Contains(
                         request.AuthorizationEncryptedResponseAlg))
            {
                // TODO invalid AuthorizationEncryptedResponseAlg
            }
            else if (!hasEmptyAuthorizationEncryptedResponseEnc &&
                     !DiscoveryDocument.AuthorizationEncryptionEncValuesSupported.Contains(
                         request.AuthorizationEncryptedResponseEnc))
            {
                // TODO invalid AuthorizationEncryptedResponseEnc
            }
            else if (!hasEmptyAuthorizationEncryptedResponseAlg && !hasEmptyAuthorizationEncryptedResponseEnc)
            {
                authorizationEncryptedResponseAlg = request.AuthorizationEncryptedResponseAlg.GetEnum<EncryptionAlg>();
                authorizationEncryptedResponseEnc = request.AuthorizationEncryptedResponseEnc.GetEnum<EncryptionEnc>();
            }
        }


        return new PostRegisterValidatedRequest
        {
            ApplicationType = applicationType,
            TokenEndpointAuthMethod = tokenEndpointAuthMethod,
            ClientName = request.ClientName,
            GrantTypes = grantTypes,
            Scope = scope,
            ResponseTypes = responseTypes,
            RedirectUris = redirectUris,
            PostLogoutRedirectUris = postLogoutRedirectUris,
            RequestUris = requestUris,
            BackchannelLogoutUri = backchannelLogoutUri,
            ClientUri = clientUri,
            PolicyUri = policyUri,
            TosUri = tosUri,
            InitiateLoginUri = initiateLoginUri,
            LogoUri = logoUri,
            Jwks = jwks,
            JwksUri = jwksUri,
            RequireSignedRequestObject = requireSignedRequestObject,
            RequireReferenceToken = requireReferenceToken,
            SubjectType = subjectType,
            DefaultMaxAge = defaultMaxAge,
            DefaultAcrValues = defaultAcrValues,
            Contacts = contacts,
            AuthorizationCodeExpiration = authorizationCodeExpiration,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration,
            ClientSecretExpiration = clientSecretExpiration,
            JwksExpiration = jwksExpiration,
            TokenEndpointAuthSigningAlg = tokenEndpointAuthSigningAlg,
            RequestObjectSigningAlg = requestObjectSigningAlg,
            RequestObjectEncryptionAlg = requestObjectEncryptionAlg,
            RequestObjectEncryptionEnc = requestObjectEncryptionEnc,
            UserinfoSignedResponseAlg = userinfoSignedResponseAlg,
            UserinfoEncryptedResponseAlg = userinfoEncryptedResponseAlg,
            UserinfoEncryptedResponseEnc = userinfoEncryptedResponseEnc,
            IdTokenSignedResponseAlg = idTokenSignedResponseAlg,
            IdTokenEncryptedResponseAlg = idTokenEncryptedResponseAlg,
            IdTokenEncryptedResponseEnc = idTokenEncryptedResponseEnc,
            AuthorizationSignedResponseAlg = authorizationSignedResponseAlg,
            AuthorizationEncryptedResponseAlg = authorizationEncryptedResponseAlg,
            AuthorizationEncryptedResponseEnc = authorizationEncryptedResponseEnc
        };
    }
}