using AuthServer.Core;
using AuthServer.Core.Abstractions;

namespace AuthServer.DomainEvents;
internal record RefreshTokenRevoked(Guid RefreshTokenId) : IDomainEvent;