namespace CatalogAPI.Application.Shared.Messaging;

public abstract class EventBase
{
    public Guid EventId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}