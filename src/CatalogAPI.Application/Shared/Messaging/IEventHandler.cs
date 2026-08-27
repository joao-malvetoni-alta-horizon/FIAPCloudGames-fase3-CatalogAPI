namespace CatalogAPI.Application.Shared.Messaging;

/// <summary>
/// Interface genérica para manipuladores de eventos de integração. Concentra a lógica de
/// aplicação de um evento, isolada da infraestrutura de mensageria.
/// </summary>
/// <typeparam name="TEvent">Tipo do evento a ser manipulado.</typeparam>
public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
