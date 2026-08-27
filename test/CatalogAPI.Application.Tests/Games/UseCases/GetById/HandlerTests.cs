using CatalogAPI.Application.Contexts.Games.UseCases.GetById;
using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Games.Queries;
using NSubstitute;

namespace CatalogAPI.Application.Tests.Games.UseCases.GetById;

public class HandlerTests
{
    private readonly IGetById _repository;
    private readonly Handler _handler;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public HandlerTests()
    {
        _repository = Substitute.For<IGetById>();
        _handler = new Handler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnGameDetail()
    {
        var game = new Game("Game Title Test", "A valid description for the game", 49.99m, GameGenre.RPG, Tomorrow);

        _repository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Game?>(game));

        var response = await _handler.Handle(new Request(game.Id), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Game);
        Assert.Equal(game.Id, response.Game.Id);
        Assert.Equal(game.Title.Value, response.Game.Title);
        Assert.Equal(game.Price.Amount, response.Game.Price);
        Assert.Equal(game.Genre, response.Game.Genre);
        Assert.Equal(game.Status, response.Game.Status);
        Assert.Equal(game.ReleaseDate, response.Game.ReleaseDate);
    }

    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldThrowGameNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Game?>(null));

        // GlobalExceptionHandler mapeia GameNotFoundException -> 404.
        await Assert.ThrowsAsync<GameNotFoundException>(
            () => _handler.Handle(new Request(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldReturnBadRequestWithoutCallingRepository()
    {
        var response = await _handler.Handle(new Request(Guid.Empty), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagate()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Game?>(new Exception("Unexpected error")));

        await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(new Request(Guid.NewGuid()), CancellationToken.None));
    }
}