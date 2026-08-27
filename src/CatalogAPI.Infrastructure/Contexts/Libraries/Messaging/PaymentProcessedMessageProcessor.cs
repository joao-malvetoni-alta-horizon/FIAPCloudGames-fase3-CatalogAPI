using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using CatalogAPI.Application.Shared.Messaging;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using FiapCloudGames.Contracts.Payments;
using FiapCloudGames.RabbitMq.Consumers;
using FiapCloudGames.RabbitMq.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CatalogAPI.Infrastructure.Contexts.Libraries.Messaging;

/// <summary>
/// Responsável por desserializar e despachar mensagens de <see cref="PaymentProcessedEvent"/>
/// para o respectivo handler, isolado da infraestrutura de conexão com o RabbitMQ (provida pelo
/// pacote FiapCloudGames.RabbitMq) e da resolução via injeção de dependência (delegada ao
/// <see cref="IEventDispatcher"/>). Não contém lógica de aplicação — apenas traduz o resultado do
/// despacho para a política de reentrega do broker.
/// </summary>
public sealed partial class PaymentProcessedMessageProcessor(
    IEventDispatcher dispatcher,
    ILogger<PaymentProcessedMessageProcessor> logger) : IMessageProcessor
{
    // Precisa casar com o formato gravado pelo publisher (PaymentsAPI) na fila:
    // case-insensitive nos nomes (camelCase/PascalCase) e enums aceitos como string ou número.
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<MessageProcessingResult> ProcessAsync(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        if (!TryDeserialize(body, out PaymentProcessedEvent? integrationEvent))
        {
            return MessageProcessingResult.PoisonMessage;
        }

        try
        {
            await dispatcher.DispatchAsync(integrationEvent, cancellationToken);
            return MessageProcessingResult.Success;
        }
        catch (GameNotFoundException ex)
        {
            // O jogo não existe no catálogo: reprocessar não vai adiantar, descarta sem reenfileirar.
            LogGameNotInCatalog(ex, integrationEvent.GameId, integrationEvent.UserId);
            return MessageProcessingResult.PoisonMessage;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Reentrega concorrente do mesmo (usuário, jogo): o item já está na biblioteca, é idempotente.
            LogDuplicateDelivery(integrationEvent.GameId, integrationEvent.UserId);
            return MessageProcessingResult.PoisonMessage;
        }
        catch (Exception ex)
        {
            LogProcessingFailed(ex, integrationEvent.UserId, integrationEvent.GameId);
            return MessageProcessingResult.TransientFailure;
        }
    }

    private bool TryDeserialize(ReadOnlyMemory<byte> body, [NotNullWhen(true)] out PaymentProcessedEvent? integrationEvent)
    {
        try
        {
            integrationEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(body.Span, SerializerOptions);
        }
        catch (JsonException ex)
        {
            LogMalformedMessage(ex);
            integrationEvent = null;
            return false;
        }

        if (integrationEvent is not null)
        {
            return true;
        }

        LogEmptyMessage();
        return false;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "PaymentProcessedEvent do jogo {GameId} / usuário {UserId} referencia um jogo que não está no catálogo, descartando.")]
    private partial void LogGameNotInCatalog(Exception exception, Guid gameId, Guid userId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Jogo {GameId} já está na biblioteca do usuário {UserId}, descartando reentrega duplicada.")]
    private partial void LogDuplicateDelivery(Guid gameId, Guid userId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Erro ao processar PaymentProcessedEvent para usuário {UserId}, jogo {GameId}, mensagem será reenfileirada.")]
    private partial void LogProcessingFailed(Exception exception, Guid userId, Guid gameId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "Mensagem PaymentProcessedEvent malformada recebida, descartando.")]
    private partial void LogMalformedMessage(Exception exception);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Warning,
        Message = "Mensagem PaymentProcessedEvent vazia recebida, descartando.")]
    private partial void LogEmptyMessage();
}
