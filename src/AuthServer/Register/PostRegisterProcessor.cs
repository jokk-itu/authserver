using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Register.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Register;

internal class PostRegisterProcessor : IPostRegisterProcessor
{
    private readonly AuthorizationDbContext _authorizationDbContext;

    public PostRegisterProcessor(
        AuthorizationDbContext authorizationDbContext)
    {
        _authorizationDbContext = authorizationDbContext;
    }

    public async Task<PostRegisterResponse> Register(PostRegisterValidatedRequest postRegisterValidatedRequest, CancellationToken cancellationToken)
    {
        var client = new Client(
            postRegisterValidatedRequest.ClientName,
            postRegisterValidatedRequest.ApplicationType,
            postRegisterValidatedRequest.TokenEndpointAuthMethod)
        {
            BackchannelLogoutUri = postRegisterValidatedRequest.BackchannelLogoutUri,
            ClientUri = postRegisterValidatedRequest.ClientUri,
            PolicyUri = postRegisterValidatedRequest.PolicyUri,
            TosUri = postRegisterValidatedRequest.TosUri,
            InitiateLoginUri = postRegisterValidatedRequest.InitiateLoginUri,
            LogoUri = postRegisterValidatedRequest.LogoUri,
            Jwks = postRegisterValidatedRequest.Jwks,
            JwksUri = postRegisterValidatedRequest.JwksUri,
            RequireSignedRequestObject = postRegisterValidatedRequest.RequireSignedRequestObject,
            RequireReferenceToken = postRegisterValidatedRequest.RequireReferenceToken,
            SubjectType = postRegisterValidatedRequest.SubjectType,
            DefaultMaxAge = postRegisterValidatedRequest.DefaultMaxAge,
            AuthorizationCodeExpiration = postRegisterValidatedRequest.AuthorizationCodeExpiration,
            AccessTokenExpiration = postRegisterValidatedRequest.AccessTokenExpiration,
            RefreshTokenExpiration = postRegisterValidatedRequest.RefreshTokenExpiration,
            SecretExpiration = postRegisterValidatedRequest.ClientSecretExpiration,
            JwksExpiration = postRegisterValidatedRequest.JwksExpiration,
            TokenEndpointAuthSigningAlg = postRegisterValidatedRequest.TokenEndpointAuthSigningAlg,
            RequestObjectSigningAlg = postRegisterValidatedRequest.RequestObjectSigningAlg,
            RequestObjectEncryptionAlg = postRegisterValidatedRequest.RequestObjectEncryptionAlg,
            RequestObjectEncryptionEnc = postRegisterValidatedRequest.RequestObjectEncryptionEnc,
            UserinfoSignedResponseAlg = postRegisterValidatedRequest.UserinfoSignedResponseAlg,
            UserinfoEncryptedResponseAlg = postRegisterValidatedRequest.UserinfoEncryptedResponseAlg,
            UserinfoEncryptedResponseEnc = postRegisterValidatedRequest.UserinfoEncryptedResponseEnc,
            IdTokenSignedResponseAlg = postRegisterValidatedRequest.IdTokenSignedResponseAlg,
            IdTokenEncryptedResponseAlg = postRegisterValidatedRequest.IdTokenEncryptedResponseAlg,
            IdTokenEncryptedResponseEnc = postRegisterValidatedRequest.IdTokenEncryptedResponseEnc,
            AuthorizationSignedResponseAlg = postRegisterValidatedRequest.AuthorizationSignedResponseAlg,
            AuthorizationEncryptedResponseAlg = postRegisterValidatedRequest.AuthorizationEncryptedResponseAlg,
            AuthorizationEncryptedResponseEnc = postRegisterValidatedRequest.AuthorizationEncryptedResponseEnc
        };

        var grantTypes = await _authorizationDbContext
            .Set<GrantType>()
            .Where(gt => postRegisterValidatedRequest.GrantTypes.Contains(gt.Name))
            .ToListAsync(cancellationToken);

        var scope = await _authorizationDbContext
            .Set<Scope>()
            .Where(s => postRegisterValidatedRequest.Scope.Contains(s.Name))
            .ToListAsync(cancellationToken);

        var responseTypes = await _authorizationDbContext
            .Set<ResponseType>()
            .Where(rt => postRegisterValidatedRequest.ResponseTypes.Contains(rt.Name))
            .ToListAsync(cancellationToken);

        var redirectUris = postRegisterValidatedRequest
            .RedirectUris
            .Select(ru => new RedirectUri(ru, client))
            .ToList();

        var postLogoutRedirectUris = postRegisterValidatedRequest
            .PostLogoutRedirectUris
            .Select(plru => new PostLogoutRedirectUri(plru, client))
            .ToList();

        var requestUris = postRegisterValidatedRequest
            .RequestUris
            .Select(ru => new RequestUri(ru, client))
            .ToList();

        var contacts = postRegisterValidatedRequest
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

        // TODO DefaultAcrValues

        await _authorizationDbContext.AddAsync(client, cancellationToken);

        // TODO fill out Response
        return new PostRegisterResponse();
    }
}