using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Claim = System.Security.Claims.Claim;

namespace AuthServer.Authentication;
internal class ReferenceTokenAuthenticationHandler : AuthenticationHandler<ReferenceTokenAuthenticationOptions>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly IUserClaimService _userClaimService;

    public ReferenceTokenAuthenticationHandler(
        IOptionsMonitor<ReferenceTokenAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AuthorizationDbContext authorizationDbContext,
        IUserClaimService userClaimService)
        : base(options, logger, encoder)
    {
        _authorizationDbContext = authorizationDbContext;
        _userClaimService = userClaimService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var hasAuthorizationHeader = AuthenticationHeaderValue.TryParse(Context.Request.Headers.Authorization, out var parsedHeader);
        if (!hasAuthorizationHeader)
        {
            return AuthenticateResult.NoResult();
        }

        if (parsedHeader!.Scheme != "Bearer")
        {
            return AuthenticateResult.NoResult();
        }

        var token = parsedHeader.Parameter;

        var query = await _authorizationDbContext
            .Set<Token>()
            .Where(t => t.Reference == token)
            .Select(t => new
            {
                Token = t,
                ClientIdFromClientToken = (t as ClientToken)!.Client.Id,
                ClientIdFromGrantToken = (t as GrantToken)!.AuthorizationGrant.Client.Id,
                GrantId = (t as GrantToken)!.AuthorizationGrant.Id,
                PublicSubjectIdentifier = (t as GrantToken)!.AuthorizationGrant.Session.PublicSubjectIdentifier.Id,
                SubjectIdentifier = (t as GrantToken)!.AuthorizationGrant.SubjectIdentifier.Id,
                SessionId = (t as GrantToken)!.AuthorizationGrant.Session.Id
            })
            .SingleOrDefaultAsync();

        if (query is null)
        {
            return AuthenticateResult.NoResult();
        }

        if (query.Token.RevokedAt != null)
        {
            return AuthenticateResult.Fail("Token is revoked");
        }

        if (query.Token.IssuedAt > DateTime.UtcNow)
        {
            return AuthenticateResult.Fail("Token is not yet active");
        }

        if (query.Token.ExpiresAt < DateTime.UtcNow)
        {
            return AuthenticateResult.Fail("Token has expired");
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
            claims.Add(new Claim(ClaimNameConstants.Sub, query.SubjectIdentifier));

            var userClaims = await _userClaimService.GetClaims(query.PublicSubjectIdentifier, CancellationToken.None);
            claims.AddRange(userClaims);
        }
        else if (query.Token is ClientToken)
        {
            claims.Add(new Claim(ClaimNameConstants.ClientId, query.ClientIdFromClientToken));
            claims.Add(new Claim(ClaimNameConstants.Sub, query.ClientIdFromClientToken));
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
        var claimsIdentity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(claimsIdentity);
        var authenticationTicket = new AuthenticationTicket(principal, authenticationProperties, ReferenceTokenAuthenticationDefaults.AuthenticationScheme);
        return AuthenticateResult.Success(authenticationTicket);
    }
}
