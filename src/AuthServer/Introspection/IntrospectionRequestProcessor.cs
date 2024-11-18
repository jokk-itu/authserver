using AuthServer.Authentication.Abstractions;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Metrics;
using AuthServer.Metrics.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Introspection;
internal class IntrospectionRequestProcessor : IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IUserClaimService _userClaimService;
    private readonly IMetricService _metricService;

    public IntrospectionRequestProcessor(
        AuthorizationDbContext identityContext,
        IUserClaimService userClaimService,
        IMetricService metricService)
    {
        _identityContext = identityContext;
        _userClaimService = userClaimService;
        _metricService = metricService;
    }

    public async Task<IntrospectionResponse> Process(IntrospectionValidatedRequest request, CancellationToken cancellationToken)
    {
        var query = await _identityContext
            .Set<Token>()
            .Where(x => x.Reference == request.Token)
            .Select(x => new TokenQuery
            {
                Token = x,
                SubjectFromGrantToken = (x as GrantToken)!.AuthorizationGrant.Subject,
                SubjectFromClientToken = (x as ClientAccessToken)!.Client.Id,
                SubjectIdentifier = (x as GrantToken)!.AuthorizationGrant.Session.SubjectIdentifier.Id
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        var isInvalidToken = query is null;
        var hasExceededExpiration = query?.Token.ExpiresAt < DateTime.UtcNow;
        var isRevoked = query?.Token.RevokedAt is not null;

        var scope = query?.Token.Scope?.Split(' ') ?? [];
        var hasInsufficientScope = !request.Scope.Intersect(scope).Any();

        /*
         * If active is false, then the requesting client does not need to know more.
         * Therefore, the other optional properties are not set.
         */
        if (isInvalidToken || hasExceededExpiration || isRevoked || hasInsufficientScope)
        {
            return new IntrospectionResponse
            {
                Active = false
            };
        }

        var token = query!.Token;
        string? username = null;
        if (query.SubjectIdentifier is not null)
        {
            username = await _userClaimService.GetUsername(query.SubjectIdentifier, cancellationToken);
        }

        var subject = query.SubjectFromGrantToken ?? query.SubjectFromClientToken;

        _metricService.AddIntrospectedToken(query.Token is RefreshToken ? TokenTypeTag.RefreshToken : TokenTypeTag.AccessToken);

        return new IntrospectionResponse
        {
            Active = token.RevokedAt is null,
            JwtId = token.Id.ToString(),
            ClientId = request.ClientId,
            ExpiresAt = token.ExpiresAt?.ToUnixTimeSeconds(),
            Issuer = token.Issuer,
            Audience = token.Audience.Split(' '),
            IssuedAt = token.IssuedAt.ToUnixTimeSeconds(),
            NotBefore = token.NotBefore.ToUnixTimeSeconds(),
            Scope = token.Scope,
            Subject = subject,
            TokenType = token.TokenType.GetDescription(),
            Username = username
        };
    }

    private sealed class TokenQuery
    {
        public required Token Token { get; init; }
        public required string? SubjectFromGrantToken { get; init; }
        public required string? SubjectFromClientToken { get; init; }
        public required string? SubjectIdentifier { get; init; }
    }
}