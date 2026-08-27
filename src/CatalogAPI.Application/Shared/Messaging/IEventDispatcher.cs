namespace CatalogAPI.Application.Shared.Messaging;

/// <summary>
/// Resolve e invoca o <see cref="IEventHandler{TEvent}"/> registrado para um evento de integração,
/// isolando os consumidores de mensageria dos detalhes de resolução via injeção de dependência.
/// </summary>
public interface IEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken);
}
