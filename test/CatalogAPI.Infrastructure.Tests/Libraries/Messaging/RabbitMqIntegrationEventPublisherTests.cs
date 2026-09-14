using CatalogAPI.Infrastructure.Contexts.Libraries.Messaging;
using FiapCloudGames.Contracts.Catalog;
using FiapCloudGames.RabbitMq.Publishers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CatalogAPI.Infrastructure.Tests.Libraries.Messaging;

/// <summary>
/// Testes unitários do <see cref="RabbitMqIntegrationEventPublisher"/>, focados na resolução de
/// rota e na injeção dos headers de trace distribuído. Sem o agente New Relic carregado no
/// processo de teste, <c>GetAgent().CurrentTransaction</c> devolve um <c>NoOpTransaction</c>: o
/// que se verifica aqui é que a sobrecarga com headers é usada e que a ausência de transação
/// ativa não quebra a publicação (critério de aceite 5).
/// </summary>
public class RabbitMqIntegrationEventPublisherTests
{
    private readonly IRabbitMqPublisher _publisher = Substitute.For<IRabbitMqPublisher>();
    private readonly RabbitMqIntegrationEventPublisher _sut;

    public RabbitMqIntegrationEventPublisherTests()
    {
        _sut = new RabbitMqIntegrationEventPublisher(
            _publisher, Substitute.For<ILogger<RabbitMqIntegrationEventPublisher>>());
    }

    private static OrderPlacedEvent AnOrder() => new(Guid.NewGuid(), Guid.NewGuid(), 59.90m);

    [Fact]
    public async Task PublishAsync_ShouldUseHeadersOverloadWithNonNullHeaderDictionary()
    {
        var orderPlaced = AnOrder();

        await _sut.PublishAsync(orderPlaced, CancellationToken.None);

        await _publisher.Received(1).PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<OrderPlacedEvent>(e => e.EventId == orderPlaced.EventId),
            Arg.Is<IDictionary<string, object?>?>(h => h != null),
            Arg.Any<CancellationToken>());

        // A sobrecarga antiga (sem headers) não pode mais ser usada: seria uma mensagem sem trace.
        await _publisher.DidNotReceive().PublishAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WithoutActiveNewRelicTransaction_ShouldNotThrowAndStillPublish()
    {
        var exception = await Record.ExceptionAsync(() => _sut.PublishAsync(AnOrder(), CancellationToken.None));

        Assert.Null(exception);
        await _publisher.Received(1).PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<OrderPlacedEvent>(),
            Arg.Any<IDictionary<string, object?>?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ShouldResolveRouteFromIntegrationEventRouteAttribute()
    {
        await _sut.PublishAsync(AnOrder(), CancellationToken.None);

        await _publisher.Received(1).PublishAsync(
            Arg.Is<string>(exchange => !string.IsNullOrWhiteSpace(exchange)),
            Arg.Is<string>(routingKey => !string.IsNullOrWhiteSpace(routingKey)),
            Arg.Any<OrderPlacedEvent>(),
            Arg.Any<IDictionary<string, object?>?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WhenBrokerFails_ShouldSwallowExceptionAndLog()
    {
        _publisher.PublishAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OrderPlacedEvent>(),
            Arg.Any<IDictionary<string, object?>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("broker down")));

        var exception = await Record.ExceptionAsync(() => _sut.PublishAsync(AnOrder(), CancellationToken.None));

        Assert.Null(exception);
    }
}
