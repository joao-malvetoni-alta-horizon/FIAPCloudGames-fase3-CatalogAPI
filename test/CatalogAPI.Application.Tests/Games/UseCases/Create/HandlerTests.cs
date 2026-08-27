using CatalogAPI.Application.Contexts.Games.UseCases.Create;
using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Shared;
using NSubstitute;

namespace CatalogAPI.Application.Tests.Games.UseCases.Create;

public class HandlerTests
{
    private readonly ICreate _repository;
    private readonly Handler _handler;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public HandlerTests()
    {
        _repository = Substitute.For<ICreate>();
        _handler = new Handler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnCreatedResponse()
    {
        var request = new Request("Game Title Test", "A valid description for the game", 49.99m, GameGenre.Action, Tomorrow);
        var game = new Game(request.Title, request.Description, request.Price, request.Genre, request.ReleaseDate);

        _repository.CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(game));

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(201, response.StatusCode);
        Assert.NotNull(response.Game);
        Assert.Equal(request.Title, response.Game.Title);
        Assert.Equal(request.Price, response.Game.Price);
        Assert.Equal(request.Genre, response.Game.Genre);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldReturnBadRequestWithoutCallingRepository()
    {
        var request = new Request("", "", -1m, GameGenre.Action, Tomorrow);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        Assert.NotNull(response.Notifications);
        await _repository.DidNotReceive().CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsDomainException_ShouldPropagate()
    {
        var request = new Request("Game Title Test", "A valid description for the game", 49.99m, GameGenre.Action, Tomorrow);

        _repository.CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Game>(new DomainException("Domain rule violated")));

        // Exceções propagam para o GlobalExceptionHandler (que mapeia DomainException -> 400).
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagate()
    {
        var request = new Request("Game Title Test", "A valid description for the game", 49.99m, GameGenre.Action, Tomorrow);

        _repository.CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Game>(new Exception("Unexpected error")));

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
    }
}