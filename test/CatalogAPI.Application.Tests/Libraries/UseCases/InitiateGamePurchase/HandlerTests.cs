using CatalogAPI.Application.Contexts.Libraries.UseCases.InitiateGamePurchase;
using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Exceptions;
using CatalogAPI.Domain.Contexts.Libraries.Queries;
using CatalogAPI.Domain.Shared;
using CatalogAPI.Application.Shared.Messaging;
using FiapCloudGames.Contracts.Catalog;
using NSubstitute;

namespace CatalogAPI.Application.Tests.Libraries.UseCases.InitiateGamePurchase;

public class HandlerTests
{
    private readonly IGetGameAndCheckOwnership _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly Handler _handler;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public HandlerTests()
    {
        _repository = Substitute.For<IGetGameAndCheckOwnership>();
        _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _handler = new Handler(_repository, _eventPublisher);
    }

    private static Game NewGame() =>
        new("Game Title Test", "A valid description", 49.99m, GameGenre.Action, Tomorrow);

    [Fact]
    public async Task Handle_WithValidRequest_ShouldPublishOrderCreatedEventAndReturnAccepted()
    {
        var game = NewGame();
        _repository.ExecuteAsync(Arg.Any<LibraryItem>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(Game?, bool)>((game, false)));

        var request = new Request(Guid.NewGuid(), Guid.NewGuid());
        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(202, response.StatusCode);
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<OrderPlacedEvent>(e => e.GameId == game.Id && e.Price == game.Price.Amount),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldReturnBadRequestWithoutTouchingRepository()
    {
        var request = new Request(Guid.Empty, Guid.Empty);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        await _repository.DidNotReceive().ExecuteAsync(Arg.Any<LibraryItem>(), Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldThrowGameNotFoundAndNotPublish()
    {
        _repository.ExecuteAsync(Arg.Any<LibraryItem>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(Game?, bool)>((null, false)));

        // GlobalExceptionHandler mapeia GameNotFoundException -> 404.
        await Assert.ThrowsAsync<GameNotFoundException>(
            () => _handler.Handle(new Request(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));

        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyOwnsGame_ShouldThrowConflictAndNotPublish()
    {
        _repository.ExecuteAsync(Arg.Any<LibraryItem>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(Game?, bool)>((NewGame(), true)));

        // GlobalExceptionHandler mapeia GameAlreadyOwnedException -> 409.
        await Assert.ThrowsAsync<GameAlreadyOwnedException>(
            () => _handler.Handle(new Request(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));

        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagate()
    {
        _repository.ExecuteAsync(Arg.Any<LibraryItem>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<(Game?, bool)>(new Exception("Unexpected error")));

        await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(new Request(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }
}