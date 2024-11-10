using System.Diagnostics;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.Metrics.Abstractions;
using AuthServer.Options;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthServer.Register;

internal class RegisterRequestProcessor : IRequestProcessor<RegisterValidatedRequest, ProcessResult<RegisterResponse, Unit>>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBuilder<RegistrationTokenArguments> _registrationTokenBuilder;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IMetricService _metricService;

    public RegisterRequestProcessor(
        AuthorizationDbContext authorizationDbContext,
        IUnitOfWork unitOfWork,
        ITokenBuilder<RegistrationTokenArguments> registrationTokenBuilder,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IMetricService metricService)
    {
        _authorizationDbContext = authorizationDbContext;
        _unitOfWork = unitOfWork;
        _registrationTokenBuilder = registrationTokenBuilder;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _metricService = metricService;
    }

    public async Task<ProcessResult<RegisterResponse, Unit>> Process(RegisterValidatedRequest request,
        CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        Client client;
        if (request.Method == HttpMethod.Post)
        {
            client = new Client(request.ClientName, request.ApplicationType, request.TokenEndpointAuthMethod);
            _authorizationDbContext.Add(client);
        }
        else if (request.Method == HttpMethod.Delete)
        {
            await DeleteClient(request.ClientId, cancellationToken);
            _metricService.AddClientDelete(stopWatch.ElapsedMilliseconds);
            stopWatch.Restart();
            return Unit.Value;
        }
        else
        {
            client = await _authorizationDbContext
                .Set<Client>()
                .Where(x => x.Id == request.ClientId)
                .Include(c => c.GrantTypes)
                .Include(c => c.ResponseTypes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.RequestUris)
                .Include(c => c.Contacts)
                .Include(c => c.Scopes)
                .Include(x => x.ClientAuthenticationContextReferences)
                .ThenInclude(x => x.AuthenticationContextReference)
                .SingleAsync(cancellationToken);
            
            _metricService.AddClientGet(stopWatch.ElapsedMilliseconds, request.ClientId);
            stopWatch.Restart();
        }

        if (request.Method == HttpMethod.Put)
        {
            ClearRelations(client);
        }

        if (request.Method == HttpMethod.Put || request.Method == HttpMethod.Post)
        {
            SetValues(request, client);
            await SetRelations(request, client, cancellationToken);
            await SetSectorIdentifier(request, client, cancellationToken);
        }

        if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Put)
        {
            var registrationAccessToken = await _authorizationDbContext
                .Set<RegistrationToken>()
                .Where(t => t.Reference == request.RegistrationAccessToken)
                .SingleAsync(cancellationToken);

            registrationAccessToken.Revoke();
        }

        var plainSecret = UpdateSecret(client);

        if (request.Method == HttpMethod.Post)
        {
            await _unitOfWork.SaveChanges(cancellationToken);
        }

        _metricService.AddClientUpdate(stopWatch.ElapsedMilliseconds, request.ClientId);
        stopWatch.Stop();

        var registrationToken = await _registrationTokenBuilder.BuildToken(new RegistrationTokenArguments
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
            ApplicationType = client.ApplicationType,
            TokenEndpointAuthMethod = client.TokenEndpointAuthMethod,
            ClientName = client.Name,
            GrantTypes = client.GrantTypes.Select(gt => gt.Name).ToList(),
            Scope = client.Scopes.Select(s => s.Name).ToList(),
            ResponseTypes = client.ResponseTypes.Select(rt => rt.Name).ToList(),
            RedirectUris = client.RedirectUris.Select(s => s.Uri).ToList(),
            PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(s => s.Uri).ToList(),
            RequestUris = client.RequestUris.Select(s => s.Uri).ToList(),
            BackchannelLogoutUri = client.BackchannelLogoutUri,
            ClientUri = client.ClientUri,
            PolicyUri = client.PolicyUri,
            TosUri = client.TosUri,
            InitiateLoginUri = client.InitiateLoginUri,
            LogoUri = client.LogoUri,
            Jwks = string.IsNullOrEmpty(client.JwksUri) ? client.Jwks : null,
            JwksUri = client.JwksUri,
            RequireSignedRequestObject = client.RequireSignedRequestObject,
            RequireReferenceToken = client.RequireReferenceToken,
            RequirePushedAuthorizationRequests = client.RequirePushedAuthorizationRequests,
            SubjectType = client.SubjectType,
            DefaultMaxAge = client.DefaultMaxAge,
            DefaultAcrValues = client.ClientAuthenticationContextReferences
                .OrderBy(x => x.Order)
                .Select(x => x.AuthenticationContextReference)
                .Select(x => x.Name)
                .ToList(),
            Contacts = client.Contacts.Select(c => c.Email).ToList(),
            AuthorizationCodeExpiration = client.AuthorizationCodeExpiration,
            AccessTokenExpiration = client.AccessTokenExpiration,
            RefreshTokenExpiration = client.RefreshTokenExpiration,
            ClientSecretExpiration = client.SecretExpiration,
            JwksExpiration = client.JwksExpiration,
            RequestUriExpiration = client.RequestUriExpiration,
            TokenEndpointAuthSigningAlg = client.TokenEndpointAuthSigningAlg,
            RequestObjectSigningAlg = client.RequestObjectSigningAlg,
            RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
            RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
            UserinfoSignedResponseAlg = client.UserinfoSignedResponseAlg,
            UserinfoEncryptedResponseAlg = client.UserinfoEncryptedResponseAlg,
            UserinfoEncryptedResponseEnc = client.UserinfoEncryptedResponseEnc,
            IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
            IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
            IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc
        };
    }

    private static string? UpdateSecret(Client client)
    {
        string? plainSecret = null;
        if (client.TokenEndpointAuthMethod != TokenEndpointAuthMethod.None)
        {
            plainSecret = CryptographyHelper.GetRandomString(32);
            var hashedSecret = CryptographyHelper.HashPassword(plainSecret);
            client.SetSecret(hashedSecret);
        }

        return plainSecret;
    }

    private static void ClearRelations(Client client)
    {
        client.GrantTypes.Clear();
        client.ResponseTypes.Clear();
        client.RedirectUris.Clear();
        client.PostLogoutRedirectUris.Clear();
        client.RequestUris.Clear();
        client.Contacts.Clear();
        client.Scopes.Clear();
    }

    private static void SetValues(RegisterValidatedRequest request, Client client)
    {
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
        client.RequireSignedRequestObject = request.RequireSignedRequestObject;
        client.RequireReferenceToken = request.RequireReferenceToken;
        client.RequirePushedAuthorizationRequests = request.RequirePushedAuthorizationRequests;
        client.SubjectType = request.SubjectType;
        client.DefaultMaxAge = request.DefaultMaxAge;
        client.AuthorizationCodeExpiration = request.AuthorizationCodeExpiration;
        client.AccessTokenExpiration = request.AccessTokenExpiration;
        client.RefreshTokenExpiration = request.RefreshTokenExpiration;
        client.SecretExpiration = request.ClientSecretExpiration;
        client.JwksExpiration = request.JwksExpiration;
        client.RequestUriExpiration = request.RequestUriExpiration;
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
    }

    private async Task SetRelations(RegisterValidatedRequest request, Client client, CancellationToken cancellationToken)
    {
        var grantTypes = await _authorizationDbContext
            .Set<GrantType>()
            .Where(gt => request.GrantTypes.Contains(gt.Name))
            .ToListAsync(cancellationToken);

        var scopes = await  _authorizationDbContext
            .Set<Scope>()
            .Where(s => request.Scope.Contains(s.Name))
            .ToListAsync(cancellationToken);

        var responseTypes = await _authorizationDbContext
            .Set<ResponseType>()
            .Where(rt => request.ResponseTypes.Contains(rt.Name))
            .ToListAsync(cancellationToken);

        var authenticationContextReferences = await _authorizationDbContext
            .Set<AuthenticationContextReference>()
            .Where(acr => request.DefaultAcrValues.Contains(acr.Name))
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

        var clientAuthenticationContextReferences = request.DefaultAcrValues
            .Select((x, i) => new ClientAuthenticationContextReference(client, authenticationContextReferences.Single(y => y.Name == x), i))
            .ToList();

        client.GrantTypes = grantTypes;
        client.Scopes = scopes;
        client.ResponseTypes = responseTypes;
        client.RedirectUris = redirectUris;
        client.PostLogoutRedirectUris = postLogoutRedirectUris;
        client.RequestUris = requestUris;
        client.Contacts = contacts;
        client.ClientAuthenticationContextReferences = clientAuthenticationContextReferences;
    }

    private async Task DeleteClient(string clientId, CancellationToken cancellationToken)
    {
        await _authorizationDbContext
            .Set<ClientToken>()
            .Where(x => x.Client.Id == clientId)
            .ExecuteDeleteAsync(cancellationToken);

        await _authorizationDbContext
            .Set<GrantToken>()
            .Where(x => x.AuthorizationGrant.Client.Id == clientId)
            .ExecuteDeleteAsync(cancellationToken);

        await _authorizationDbContext
            .Set<ConsentGrant>()
            .Where(x => x.Client.Id == clientId)
            .ExecuteDeleteAsync(cancellationToken);

        await _authorizationDbContext
            .Set<Nonce>()
            .Where(x => x.AuthorizationGrant.Client.Id == clientId)
            .ExecuteDeleteAsync(cancellationToken);

        await _authorizationDbContext
            .Set<AuthorizationCode>()
            .Where(x => x.AuthorizationGrant.Client.Id == clientId)
            .ExecuteDeleteAsync(cancellationToken);

        await _authorizationDbContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Client.Id == clientId)
            .ExecuteDeleteAsync(cancellationToken);

        await _authorizationDbContext
            .Set<Client>()
            .Where(x => x.Id == clientId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task SetSectorIdentifier(RegisterValidatedRequest request, Client client, CancellationToken cancellationToken)
    {
        if (request.SectorIdentifierUri is null
            || client.SectorIdentifier?.Uri == request.SectorIdentifierUri)
        {
            return;
        }
        
        var sectorIdentifier = await _authorizationDbContext
            .Set<SectorIdentifier>()
            .Where(x => x.Uri == request.SectorIdentifierUri)
            .SingleOrDefaultAsync(cancellationToken);

        if (sectorIdentifier is null)
        { 
            sectorIdentifier = new SectorIdentifier(request.SectorIdentifierUri);
        }

        client.SectorIdentifier = sectorIdentifier;
    }
}