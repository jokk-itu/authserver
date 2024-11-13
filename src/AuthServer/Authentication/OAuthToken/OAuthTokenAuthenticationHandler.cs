using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Claim = System.Security.Claims.Claim;

namespace AuthServer.Authentication.OAuthToken;
internal class OAuthTokenAuthenticationHandler : AuthenticationHandler<OAuthTokenAuthenticationOptions>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly IUserClaimService _userClaimService;
    private readonly IOptionsMonitor<JwksDocument> _jwksDocumentOptions;
    private readonly IOptionsMonitor<DiscoveryDocument> _discoveryDocumentOptions;

    public OAuthTokenAuthenticationHandler(
        IOptionsMonitor<OAuthTokenAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AuthorizationDbContext authorizationDbContext,
        IUserClaimService userClaimService,
        IOptionsMonitor<JwksDocument> jwksDocumentOptions,
        IOptionsMonitor<DiscoveryDocument> discoveryDocumentOptions)
        : base(options, logger, encoder)
    {
        _authorizationDbContext = authorizationDbContext;
        _userClaimService = userClaimService;
        _jwksDocumentOptions = jwksDocumentOptions;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var hasAuthorizationHeader = AuthenticationHeaderValue.TryParse(Context.Request.Headers.Authorization, out var parsedHeader);
        if (!hasAuthorizationHeader)
        {
            return AuthenticateResult.NoResult();
        }

        var token = parsedHeader!.Parameter!;

        if (parsedHeader.Scheme != "Bearer")
        {
            return AuthenticateResult.NoResult();
        }

        ClaimsIdentity? claimsIdentity;
        AuthenticateResult? result;
        if (TokenHelper.IsJsonWebToken(token))
        {
            (claimsIdentity, result) = await AuthenticateJsonWebToken(token);
        }
        else
        {
            (claimsIdentity, result) = await AuthenticateReferenceToken(token);
        }

        if (result is not null)
        {
            return result;
        }

        var authenticationProperties = new AuthenticationProperties();
        authenticationProperties.StoreTokens(
        [
            new AuthenticationToken
            {
                Name = Parameter.AccessToken,
                Value = token!
            }
        ]);
        
        var principal = new ClaimsPrincipal(claimsIdentity!);
        var authenticationTicket = new AuthenticationTicket(principal, authenticationProperties, OAuthTokenAuthenticationDefaults.AuthenticationScheme);
        return AuthenticateResult.Success(authenticationTicket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authenticationResult = await HandleAuthenticateOnceSafeAsync();
        if (authenticationResult.None)
        {
            Response.Headers.WWWAuthenticate= "Bearer error=\"invalid_request\"";
        }
        else if (authenticationResult.Failure is not null)
        {
            Response.Headers.WWWAuthenticate = "Bearer error=\"invalid_token\"";
        }
        else
        {
            throw new InvalidOperationException("Challenge must happened from failure or none");
        }

        Response.StatusCode = 401;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = "Bearer error=\"insufficient_scope\"";
        Response.StatusCode = 403;
        return Task.CompletedTask;
    }

    private async Task<(ClaimsIdentity?, AuthenticateResult?)> AuthenticateReferenceToken(string token)
    {
        var query = await _authorizationDbContext
            .Set<Token>()
            .Where(t => t.Reference == token)
            .Select(t => new
            {
                Token = t,
                ClientIdFromClientToken = (t as ClientToken)!.Client.Id,
                ClientIdFromGrantToken = (t as GrantToken)!.AuthorizationGrant.Client.Id,
                GrantId = (t as GrantToken)!.AuthorizationGrant.Id,
                SubjectIdentifier = (t as GrantToken)!.AuthorizationGrant.Session.SubjectIdentifier.Id,
                (t as GrantToken)!.AuthorizationGrant.Subject,
                SessionId = (t as GrantToken)!.AuthorizationGrant.Session.Id
            })
            .SingleOrDefaultAsync();

        if (query is null)
        {
            Logger.LogDebug("Token {Token} does not exist", token);
            return (null, AuthenticateResult.NoResult());
        }

        if (query.Token.RevokedAt != null)
        {
            return (null, AuthenticateResult.Fail("Token is revoked"));
        }

        if (query.Token.IssuedAt > DateTime.UtcNow)
        {
            return (null, AuthenticateResult.Fail("Token is not yet active"));
        }

        if (query.Token.ExpiresAt < DateTime.UtcNow)
        {
            return (null, AuthenticateResult.Fail("Token has expired"));
        }

        var claims = new List<Claim>();
        if (query.Token.Scope is not null)
        {
            claims.Add(new Claim(ClaimNameConstants.Scope, query.Token.Scope));
        }

        if (query.Token is GrantToken)
        {
            claims.Add(new Claim(ClaimNameConstants.GrantId, query.GrantId));
            claims.Add(new Claim(ClaimNameConstants.ClientId, query.ClientIdFromGrantToken));
            claims.Add(new Claim(ClaimNameConstants.Sid, query.SessionId));
            claims.Add(new Claim(ClaimNameConstants.Sub, query.Subject));

            var userClaims = await _userClaimService.GetClaims(query.SubjectIdentifier, CancellationToken.None);
            claims.AddRange(userClaims);
        }
        else if (query.Token is ClientToken)
        {
            claims.Add(new Claim(ClaimNameConstants.ClientId, query.ClientIdFromClientToken));
            claims.Add(new Claim(ClaimNameConstants.Sub, query.ClientIdFromClientToken));
        }

        return (new ClaimsIdentity(claims), null);
    }

    private async Task<(ClaimsIdentity?, AuthenticateResult?)> AuthenticateJsonWebToken(string token)
    {
        var tokenHandler = new JsonWebTokenHandler();
        var tokenSigningKey = _jwksDocumentOptions.CurrentValue.GetTokenSigningKey();
        var tokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.FromSeconds(0),
            IssuerSigningKey = tokenSigningKey.Key,
            ValidIssuer = _discoveryDocumentOptions.CurrentValue.Issuer,
            ValidAudience = _discoveryDocumentOptions.CurrentValue.Issuer,
            ValidTypes = [TokenTypeHeaderConstants.AccessToken],
            ValidAlgorithms = [tokenSigningKey.Alg.GetDescription()],
            RoleClaimType = ClaimNameConstants.Roles,
            NameClaimType = ClaimNameConstants.Name
        };
        var validationResult = await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
        if (validationResult.IsValid)
        {
            return (validationResult.ClaimsIdentity, null);
        }

        Logger.LogWarning(validationResult.Exception, "Token validation failed");
        return (null, AuthenticateResult.Fail("Token is not valid"));
    }
}
