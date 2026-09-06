using CatalogAPI.Application.Shared.Cache;
using CatalogAPI.Application.Shared.Messaging;
using CatalogAPI.Domain.Contexts.Libraries.Commands;
using FiapCloudGames.Contracts.Payments;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.Contexts.Libraries.EventHandlers;

/// <summary>
/// Manipula o <see cref="PaymentProcessedEvent"/> publicado pela PaymentsAPI. Se o pagamento foi
/// aprovado, adiciona o jogo à biblioteca do usuário (operação idempotente no repositório); caso
/// contrário, nada é feito. Exceções são propagadas para o processor decidir a política de reentrega.
/// </summary>
public partial class PaymentProcessedEventHandler(
    IGamePurchase repository,
    ILogger<PaymentProcessedEventHandler> logger) : IEventHandler<PaymentProcessedEvent>
{
    public async Task HandleAsync(PaymentProcessedEvent paymentProcessed, CancellationToken cancellationToken = default)
    {
        if (paymentProcessed.Status is not PaymentStatus.Approved)
        {
            LogPaymentNotApproved(paymentProcessed.GameId, paymentProcessed.UserId, paymentProcessed.Status);
            return;
        }

        await repository.ExecuteAsync(paymentProcessed.UserId, paymentProcessed.GameId, cancellationToken);

        LogGameAddedToLibrary(paymentProcessed.GameId, paymentProcessed.UserId);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Pagamento do jogo {GameId} / usuário {UserId} não foi aprovado ({Status}). O jogo não será adicionado à biblioteca.")]
    private partial void LogPaymentNotApproved(Guid gameId, Guid userId, PaymentStatus status);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Jogo {GameId} adicionado à biblioteca do usuário {UserId}.")]
    private partial void LogGameAddedToLibrary(Guid gameId, Guid userId);
}
