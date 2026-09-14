using System.Text;
using System.Text.Json;
using CatalogAPI.Application.Shared.Messaging;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Infrastructure.Contexts.Libraries.Messaging;
using FiapCloudGames.Contracts.Payments;
using FiapCloudGames.RabbitMq.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NSubstitute;

namespace CatalogAPI.Infrastructure.Tests.Libraries.Messaging;

/// <summary>
/// Testes unitários do <see cref="PaymentProcessedMessageProcessor"/>, isolando a desserialização
/// e o mapeamento de falhas para <see cref="MessageProcessingResult"/> da resolução de handlers
/// (delegada ao <see cref="IEventDispatcher"/>).
/// </summary>
public class PaymentProcessedMessageProcessorTests
{
    private readonly IEventDispatcher _dispatcher = Substitute.For<IEventDispatcher>();
    private readonly PaymentProcessedMessageProcessor _processor;

    public PaymentProcessedMessageProcessorTests()
    {
        _processor = new PaymentProcessedMessageProcessor(
            _dispatcher, Substitute.For<ILogger<PaymentProcessedMessageProcessor>>());
    }

    // Mensagem publicada sem headers: o consumidor do pacote entrega um dicionário vazio.
    private static readonly IReadOnlyDictionary<string, string?> NoHeaders =
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    private static byte[] Serialize(PaymentProcessedEvent paymentProcessed) => JsonSerializer.SerializeToUtf8Bytes(paymentProcessed);

    [Fact]
    public async Task ProcessAsync_WithValidMessage_ShouldDispatchAndReturnSuccess()
    {
        var paymentProcessed = new PaymentProcessedEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Approved);

        var result = await _processor.ProcessAsync(Serialize(paymentProcessed), NoHeaders, CancellationToken.None);

        Assert.Equal(MessageProcessingResult.Success, result);
        await _dispatcher.Received(1).DispatchAsync(
            Arg.Is<PaymentProcessedEvent>(e => e.UserId == paymentProcessed.UserId && e.GameId == paymentProcessed.GameId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WithMalformedJson_ShouldReturnPoisonMessageAndNotDispatch()
    {
        var body = Encoding.UTF8.GetBytes("{ not valid json");

        var result = await _processor.ProcessAsync(body, NoHeaders, CancellationToken.None);

        Assert.Equal(MessageProcessingResult.PoisonMessage, result);
        await _dispatcher.DidNotReceive().DispatchAsync(Arg.Any<PaymentProcessedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenGameNotInCatalog_ShouldReturnPoisonMessage()
    {
        var paymentProcessed = new PaymentProcessedEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Approved);
        _dispatcher.DispatchAsync(Arg.Any<PaymentProcessedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new GameNotFoundException("Game not found in the catalog.")));

        var result = await _processor.ProcessAsync(Serialize(paymentProcessed), NoHeaders, CancellationToken.None);

        Assert.Equal(MessageProcessingResult.PoisonMessage, result);
    }

    [Fact]
    public async Task ProcessAsync_WhenGameAlreadyOwned_ShouldReturnPoisonMessage()
    {
        var paymentProcessed = new PaymentProcessedEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Approved);
        var uniqueViolation = new PostgresException(
            "duplicate key value violates unique constraint", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation);
        _dispatcher.DispatchAsync(Arg.Any<PaymentProcessedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new DbUpdateException("duplicate", uniqueViolation)));

        var result = await _processor.ProcessAsync(Serialize(paymentProcessed), NoHeaders, CancellationToken.None);

        Assert.Equal(MessageProcessingResult.PoisonMessage, result);
    }

    [Fact]
    public async Task ProcessAsync_WhenDispatcherThrowsTransiently_ShouldReturnTransientFailure()
    {
        var paymentProcessed = new PaymentProcessedEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Approved);
        _dispatcher.DispatchAsync(Arg.Any<PaymentProcessedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new TimeoutException("Connection timeout")));

        var result = await _processor.ProcessAsync(Serialize(paymentProcessed), NoHeaders, CancellationToken.None);

        Assert.Equal(MessageProcessingResult.TransientFailure, result);
    }

    [Fact]
    public async Task ProcessAsync_WithDistributedTraceHeaders_ShouldAcceptThemAndDispatch()
    {
        var paymentProcessed = new PaymentProcessedEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Approved);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["traceparent"] = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01",
            ["tracestate"] = "1234@nr=0-0-1234-5678-b7ad6b7169203331----1518469636035",
            ["newrelic"] = "eyJ2IjpbMCwxXX0=",
        };

        var result = await _processor.ProcessAsync(Serialize(paymentProcessed), headers, CancellationToken.None);

        Assert.Equal(MessageProcessingResult.Success, result);
        await _dispatcher.Received(1).DispatchAsync(Arg.Any<PaymentProcessedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WithNullValuedHeader_ShouldNotThrowAndStillDispatch()
    {
        // O consumidor normaliza headers AMQP para string?, então um valor nulo é possível:
        // o getter passado ao agente precisa devolver vazio em vez de estourar.
        var paymentProcessed = new PaymentProcessedEvent(Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Approved);
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["traceparent"] = null,
        };

        var result = await _processor.ProcessAsync(Serialize(paymentProcessed), headers, CancellationToken.None);

        Assert.Equal(MessageProcessingResult.Success, result);
        await _dispatcher.Received(1).DispatchAsync(Arg.Any<PaymentProcessedEvent>(), Arg.Any<CancellationToken>());
    }
}
