using System.Collections.Concurrent;
using System.Reflection;
using CatalogAPI.Application.Shared.Messaging;
using FiapCloudGames.Contracts;
using FiapCloudGames.RabbitMq.Publishers;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;

namespace CatalogAPI.Infrastructure.Contexts.Libraries.Messaging;

/// <summary>
/// Adapta o IRabbitMqPublisher (pacote FiapCloudGames.RabbitMq) para o contrato
/// IIntegrationEventPublisher da camada de Application. A rota (exchange/routing key)
/// é resolvida a partir do atributo [IntegrationEventRoute] do próprio evento.
/// Também injeta o contexto de trace distribuído do New Relic nos headers da mensagem,
/// para que o consumidor do outro lado da fila continue o mesmo trace.
/// </summary>
public sealed partial class RabbitMqIntegrationEventPublisher(
    IRabbitMqPublisher publisher,
    ILogger<RabbitMqIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    // Reflection só na primeira publicação de cada tipo de evento; depois vem do cache.
    private static readonly ConcurrentDictionary<Type, (string Exchange, string RoutingKey)> RouteCache = new();

    public async Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        // Usa o tipo em runtime (e não typeof(TEvent)) para achar a rota mesmo quando
        // o evento é publicado por uma referência do tipo base IIntegrationEvent.
        var (exchange, routingKey) = ResolveRoute(integrationEvent.GetType());

        try
        {
            await publisher.PublishAsync(
                exchange, routingKey, integrationEvent, BuildDistributedTraceHeaders(), cancellationToken);
        }
        catch (Exception ex)
        {
            LogPublishFailed(ex, integrationEvent.GetType().Name, integrationEvent.EventId);
        }
    }

    /// <summary>
    /// Monta os headers de trace distribuído (traceparent, tracestate e o proprietário newrelic)
    /// a partir da transação corrente do agente. Sem transação ativa o agente devolve um
    /// NoOpTransaction: o dicionário volta vazio, o pacote publica a mensagem sem tabela de
    /// headers e nada quebra — esse é o caso de testes e de publicações fora de requisição HTTP.
    /// </summary>
    private static Dictionary<string, object?> BuildDistributedTraceHeaders()
    {
        var headers = new Dictionary<string, object?>(StringComparer.Ordinal);

        NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction
            .InsertDistributedTraceHeaders(headers, static (carrier, key, value) => carrier[key] = value);

        return headers;
    }

    private static (string Exchange, string RoutingKey) ResolveRoute(Type eventType) =>
        RouteCache.GetOrAdd(eventType, static type =>
        {
            var route = type.GetCustomAttribute<IntegrationEventRouteAttribute>()
                ?? throw new InvalidOperationException(
                    $"O evento {type.Name} não possui [IntegrationEventRoute]. " +
                    "Anote-o no pacote FiapCloudGames.Contracts com a exchange e a routing key.");

            return (route.Exchange, route.RoutingKey);
        });

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Evento {EventType} (EventId {EventId}) não pôde ser publicado e será perdido (sem padrão Outbox)")]
    private partial void LogPublishFailed(Exception exception, string eventType, Guid eventId);
}
