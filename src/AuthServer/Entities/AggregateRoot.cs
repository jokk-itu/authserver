using System.ComponentModel.DataAnnotations.Schema;
using AuthServer.Core.Abstractions;

namespace AuthServer.Core;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    [NotMapped]
    private readonly List<Func<IDomainEvent>> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<Func<IDomainEvent>> DomainEvents => _domainEvents.AsReadOnly();
    protected void Raise(Func<IDomainEvent> domainEventGetter) => _domainEvents.Add(domainEventGetter);
}