using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthServer.Register.UpdateClient;

internal class PutRegisterRequestProcessor : IRequestProcessor<PutRegisterValidatedRequest, RegisterResponse>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBuilder<RegistrationTokenArguments> _tokenBuilder;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    public PutRegisterRequestProcessor(
        AuthorizationDbContext authorizationDbContext,
        IUnitOfWork unitOfWork,
        ITokenBuilder<RegistrationTokenArguments> tokenBuilder,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _authorizationDbContext = authorizationDbContext;
        _unitOfWork = unitOfWork;
        _tokenBuilder = tokenBuilder;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public async Task<RegisterResponse> Process(PutRegisterValidatedRequest request, CancellationToken cancellationToken)
    {
        var registrationAccessToken = await _authorizationDbContext
            .Set<RegistrationToken>()
            .Where(t => t.Reference == request.RegistrationAccessToken)
            .SingleAsync(cancellationToken);

        registrationAccessToken.Revoke();

        var client = await _authorizationDbContext
            .Set<Client>()
            .Where(c => c.Id == request.ClientId)
            .Include(c => c.GrantTypes)
            .Include(c => c.ResponseTypes)
            .Include(c => c.RedirectUris)
            .Include(c => c.PostLogoutRedirectUris)
            .Include(c => c.RequestUris)
            .Include(c => c.Contacts)
            .Include(c => c.Scopes)
            .SingleAsync(cancellationToken);

        client.GrantTypes.Clear();
        client.ResponseTypes.Clear();
        client.RedirectUris.Clear();
        client.PostLogoutRedirectUris.Clear();
        client.RequestUris.Clear();
        client.Contacts.Clear();
        client.Scopes.Clear();

        client.Name = request.ClientName;
        client.ApplicationType = request.ApplicationType;
        client.TokenEndpointAuthMethod = request.TokenEndpointAuthMethod;
        client.BackchannelLogoutUri = request.BackchannelLogoutUri;
        client.ClientUri = request.ClientUri;
        client.PolicyUri = request.PolicyUri;
        client.TosUri = request.TosUri;
        client.InitiateLoginUri = request.InitiateLoginUri;
        client.LogoUri = request.LogoUri;
        client.Jwks = request.Jwks;
        client.JwksUri = request.JwksUri;
        client.DefaultAcrValues = string.Join(' ', request.DefaultAcrValues);
        client.RequireSignedRequestObject = request.RequireSignedRequestObject;
        client.RequireReferenceToken = request.RequireReferenceToken;
        client.SubjectType = request.SubjectType;
        client.DefaultMaxAge = request.DefaultMaxAge;
        client.AuthorizationCodeExpiration = request.AuthorizationCodeExpiration;
        client.AccessTokenExpiration = request.AccessTokenExpiration;
        client.RefreshTokenExpiration = request.RefreshTokenExpiration;
        client.SecretExpiration = request.ClientSecretExpiration;
        client.JwksExpiration = request.JwksExpiration;
        client.TokenEndpointAuthSigningAlg = request.TokenEndpointAuthSigningAlg;
        client.RequestObjectSigningAlg = request.RequestObjectSigningAlg;
        client.RequestObjectEncryptionAlg = request.RequestObjectEncryptionAlg;
        client.RequestObjectEncryptionEnc = request.RequestObjectEncryptionEnc;
        client.UserinfoSignedResponseAlg = request.UserinfoSignedResponseAlg;
        client.UserinfoEncryptedResponseAlg = request.UserinfoEncryptedResponseAlg;
        client.UserinfoEncryptedResponseEnc = request.UserinfoEncryptedResponseEnc;
        client.IdTokenSignedResponseAlg = request.IdTokenSignedResponseAlg;
        client.IdTokenEncryptedResponseAlg = request.IdTokenEncryptedResponseAlg;
        client.IdTokenEncryptedResponseEnc = request.IdTokenEncryptedResponseEnc;
        client.AuthorizationSignedResponseAlg = request.AuthorizationSignedResponseAlg;
        client.AuthorizationEncryptedResponseAlg = request.AuthorizationEncryptedResponseAlg;
        client.AuthorizationEncryptedResponseEnc = request.AuthorizationEncryptedResponseEnc;

        var grantTypes = await _authorizationDbContext
            .Set<GrantType>()
            .Where(gt => request.GrantTypes.Contains(gt.Name))
            .ToListAsync(cancellationToken);

        var scope = await _authorizationDbContext
            .Set<Scope>()
            .Where(s => request.Scope.Contains(s.Name))
            .ToListAsync(cancellationToken);

        var responseTypes = await _authorizationDbContext
            .Set<ResponseType>()
            .Where(rt => request.ResponseTypes.Contains(rt.Name))
            .ToListAsync(cancellationToken);

        var redirectUris = request
            .RedirectUris
            .Select(ru => new RedirectUri(ru, client))
            .ToList();

        var postLogoutRedirectUris = request
            .PostLogoutRedirectUris
            .Select(plru => new PostLogoutRedirectUri(plru, client))
            .ToList();

        var requestUris = request
            .RequestUris
            .Select(ru => new RequestUri(ru, client))
            .ToList();

        var contacts = request
            .Contacts
            .Select(c => new Contact(c, client))
            .ToList();

        client.GrantTypes = grantTypes;
        client.Scopes = scope;
        client.ResponseTypes = responseTypes;
        client.RedirectUris = redirectUris;
        client.PostLogoutRedirectUris = postLogoutRedirectUris;
        client.RequestUris = requestUris;
        client.Contacts = contacts;

        string? plainSecret = null;
        if (request.TokenEndpointAuthMethod != TokenEndpointAuthMethod.None)
        {
            plainSecret = CryptographyHelper.GetRandomString(32);
            var hashedSecret = BCrypt.HashPassword(plainSecret, BCrypt.GenerateSalt());
            client.SetSecret(hashedSecret);
        }

        // Persist the Client, as it is needed when building the token
        await _unitOfWork.SaveChanges();

        var registrationToken = await _tokenBuilder.BuildToken(
            new RegistrationTokenArguments
            {
                ClientId = client.Id
            }, cancellationToken);

        return new RegisterResponse
        {
            ClientId = client.Id,
            ClientIdIssuedAt = client.CreatedAt.Ticks,
            ClientSecret = plainSecret,
            ClientSecretExpiresAt = client.SecretExpiresAt?.Ticks ?? 0,
            RegistrationClientUri = $"{_discoveryDocumentOptions.Value.RegistrationEndpoint}?client_id={client.Id}",
            RegistrationAccessToken = registrationToken,
            ApplicationType = request.ApplicationType,
            TokenEndpointAuthMethod = request.TokenEndpointAuthMethod,
            ClientName = request.ClientName,
            GrantTypes = request.GrantTypes,
            Scope = request.Scope,
            ResponseTypes = request.ResponseTypes,
            RedirectUris = request.RedirectUris,
            PostLogoutRedirectUris = request.PostLogoutRedirectUris,
            RequestUris = request.RequestUris,
            BackchannelLogoutUri = request.BackchannelLogoutUri,
            ClientUri = request.ClientUri,
            PolicyUri = request.PolicyUri,
            TosUri = request.TosUri,
            InitiateLoginUri = request.InitiateLoginUri,
            LogoUri = request.LogoUri,
			Jwks = string.IsNullOrEmpty(request.JwksUri) ? request.Jwks : null,
			JwksUri = request.JwksUri,
			RequireSignedRequestObject = request.RequireSignedRequestObject,
            RequireReferenceToken = request.RequireReferenceToken,
            SubjectType = request.SubjectType,
            DefaultMaxAge = request.DefaultMaxAge,
            DefaultAcrValues = request.DefaultAcrValues,
            Contacts = request.Contacts,
            AuthorizationCodeExpiration = request.AuthorizationCodeExpiration,
            AccessTokenExpiration = request.AccessTokenExpiration,
            RefreshTokenExpiration = request.RefreshTokenExpiration,
            ClientSecretExpiration = request.ClientSecretExpiration,
            JwksExpiration = request.JwksExpiration,
            TokenEndpointAuthSigningAlg = request.TokenEndpointAuthSigningAlg,
            RequestObjectSigningAlg = request.RequestObjectSigningAlg,
            RequestObjectEncryptionAlg = request.RequestObjectEncryptionAlg,
            RequestObjectEncryptionEnc = request.RequestObjectEncryptionEnc,
            UserinfoSignedResponseAlg = request.UserinfoSignedResponseAlg,
            UserinfoEncryptedResponseAlg = request.UserinfoEncryptedResponseAlg,
            UserinfoEncryptedResponseEnc = request.UserinfoEncryptedResponseEnc,
            IdTokenSignedResponseAlg = request.IdTokenSignedResponseAlg,
            IdTokenEncryptedResponseAlg = request.IdTokenEncryptedResponseAlg,
            IdTokenEncryptedResponseEnc = request.IdTokenEncryptedResponseEnc,
            AuthorizationSignedResponseAlg = request.AuthorizationSignedResponseAlg,
            AuthorizationEncryptedResponseAlg = request.AuthorizationEncryptedResponseAlg,
            AuthorizationEncryptedResponseEnc = request.AuthorizationEncryptedResponseEnc
        };
    }
}