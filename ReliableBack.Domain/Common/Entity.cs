namespace ReliableBack.Domain.Common;

public class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new ();

    public Guid Id { get; protected set; } = Guid.NewGuid();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}