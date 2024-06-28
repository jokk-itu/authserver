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

namespace AuthServer.Register.CreateClient;

internal class PostRegisterRequestProcessor : IRequestProcessor<PostRegisterValidatedRequest, RegisterResponse>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly ITokenBuilder<RegistrationTokenArguments> _tokenBuilder;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    public PostRegisterRequestProcessor(
        AuthorizationDbContext authorizationDbContext,
        ITokenBuilder<RegistrationTokenArguments> tokenBuilder,
        IUnitOfWork unitOfWork,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _authorizationDbContext = authorizationDbContext;
        _tokenBuilder = tokenBuilder;
        _unitOfWork = unitOfWork;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public async Task<RegisterResponse> Process(PostRegisterValidatedRequest request, CancellationToken cancellationToken)
    {
        var client = new Client(
            request.ClientName,
            request.ApplicationType,
            request.TokenEndpointAuthMethod)
        {
            BackchannelLogoutUri = request.BackchannelLogoutUri,
            ClientUri = request.ClientUri,
            PolicyUri = request.PolicyUri,
            TosUri = request.TosUri,
            InitiateLoginUri = request.InitiateLoginUri,
            LogoUri = request.LogoUri,
            Jwks = request.Jwks,
            JwksUri = request.JwksUri,
            DefaultAcrValues = string.Join(' ', request.DefaultAcrValues),
            RequireSignedRequestObject = request.RequireSignedRequestObject,
            RequireReferenceToken = request.RequireReferenceToken,
            SubjectType = request.SubjectType,
            DefaultMaxAge = request.DefaultMaxAge,
            AuthorizationCodeExpiration = request.AuthorizationCodeExpiration,
            AccessTokenExpiration = request.AccessTokenExpiration,
            RefreshTokenExpiration = request.RefreshTokenExpiration,
            SecretExpiration = request.ClientSecretExpiration,
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

        _authorizationDbContext.Add(client);

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
            RegistrationClientUri = $"{_discoveryDocumentOptions.Value.RegistrationEndpoint}?clientId={client.Id}",
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
            Jwks = request.Jwks,
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