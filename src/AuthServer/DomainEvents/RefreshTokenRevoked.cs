using AuthServer.Core;

namespace AuthServer.DomainEvents;
internal record RefreshTokenRevoked(Guid refreshTokenId) : IDomainEvent;