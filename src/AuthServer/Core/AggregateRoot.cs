﻿using System.ComponentModel.DataAnnotations.Schema;

namespace AuthServer.Core;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    [NotMapped]
    private readonly List<Func<IDomainEvent>> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<Func<IDomainEvent>> DomainEvents => _domainEvents.AsReadOnly();
    public void Raise(Func<IDomainEvent> domainEventGetter) => _domainEvents.Add(domainEventGetter);
}