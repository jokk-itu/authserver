using AuthServer.Core.Abstractions;
using AuthServer.Enums;

namespace AuthServer.DomainEvents;

internal record SubjectTypeChanged(string ClientId, SubjectType OldSubjectType, SubjectType NewSubjectType) : IDomainEvent;