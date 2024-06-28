using AuthServer.Core;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Introspection.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Introspection;
internal class IntrospectionRequestProcessor : IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IUsernameResolver _usernameResolver;

    public IntrospectionRequestProcessor(
        AuthorizationDbContext identityContext,
        IUsernameResolver usernameResolver)
    {
        _identityContext = identityContext;
        _usernameResolver = usernameResolver;
    }

    public async Task<IntrospectionResponse> Process(IntrospectionValidatedRequest request, CancellationToken cancellationToken)
    {
        var query = await _identityContext
            .Set<Token>()
            .Where(x => x.Reference == request.Token)
            .Select(x => new TokenQuery
            {
                Token = x,
                SubjectIdentifier = (x as GrantToken)!.AuthorizationGrant.SubjectIdentifier.Id,
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (query is null)
        {
            return new IntrospectionResponse
            {
                Active = false
            };
        }

        long? expiresAt = query.Token.ExpiresAt is null
            ? null
            : new DateTimeOffset(query.Token.ExpiresAt.Value).ToUnixTimeSeconds();

        var username = await _usernameResolver.GetUsername(query.SubjectIdentifier);

        return new IntrospectionResponse
        {
            Active = query.Token.RevokedAt is null,
            JwtId = query.Token.Id.ToString(),
            ClientId = request.ClientId,
            ExpiresAt = expiresAt,
            Issuer = query.Token.Issuer,
            Audience = query.Token.Audience.Split(' '),
            IssuedAt = new DateTimeOffset(query.Token.IssuedAt).ToUnixTimeSeconds(),
            NotBefore = new DateTimeOffset(query.Token.NotBefore).ToUnixTimeSeconds(),
            Scope = query.Token.Scope,
            Subject = query.SubjectIdentifier,
            TokenType = query.Token.TokenType.GetDescription(),
            Username = username
        };
    }

    private sealed class TokenQuery
    {
        public required Token Token { get; init; }
        public required string SubjectIdentifier { get; init; }
    }
}