using CatalogAPI.Application.Contexts.Games.UseCases.Update;
using CatalogAPI.Application.Shared.Cache;
using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Shared;
using NSubstitute;

namespace CatalogAPI.Application.Tests.Games.UseCases.Update;

public class HandlerTests
{
    private readonly IUpdate _repository;
    private readonly Handler _handler;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public HandlerTests()
    {
        _repository = Substitute.For<IUpdate>();
        _handler = new Handler(_repository, Substitute.For<ICacheService>());
    }

    private static Request ValidRequest(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), "Updated Title", "Updated description text here", 59.99m, GameGenre.RPG, GameStatus.Active, Tomorrow);

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnNoContent()
    {
        var request = ValidRequest();

        _repository.UpdateAsync(
                request.Id, request.Title, request.Description, request.Price,
                request.Genre, request.Status, request.ReleaseDate, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(204, response.StatusCode);
    }

    [Fact]
    public async Task Handle_WhenGameNotFound_ShouldThrowGameNotFound()
    {
        var request = ValidRequest();

        _repository.UpdateAsync(
                Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<decimal?>(),
                Arg.Any<GameGenre?>(), Arg.Any<GameStatus?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // GlobalExceptionHandler mapeia GameNotFoundException -> 404.
        await Assert.ThrowsAsync<GameNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldReturnBadRequestWithoutCallingRepository()
    {
        var request = ValidRequest(Guid.Empty);

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        await _repository.DidNotReceive().UpdateAsync(
            Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<decimal?>(),
            Arg.Any<GameGenre?>(), Arg.Any<GameStatus?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsDomainException_ShouldPropagate()
    {
        var request = ValidRequest();

        _repository.UpdateAsync(
                Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<decimal?>(),
                Arg.Any<GameGenre?>(), Arg.Any<GameStatus?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new DomainException("Domain rule violated")));

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagate()
    {
        var request = ValidRequest();

        _repository.UpdateAsync(
                Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<decimal?>(),
                Arg.Any<GameGenre?>(), Arg.Any<GameStatus?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Unexpected error")));

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
    }
}