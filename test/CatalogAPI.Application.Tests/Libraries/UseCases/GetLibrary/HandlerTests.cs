using CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary;
using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Libraries.Queries;
using NSubstitute;

namespace CatalogAPI.Application.Tests.Libraries.UseCases.GetLibrary;

public class HandlerTests
{
    private readonly IGetLibrary _query;
    private readonly Handler _handler;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public HandlerTests()
    {
        _query = Substitute.For<IGetLibrary>();
        _handler = new Handler(_query);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnPagedLibrary()
    {
        var games = new List<Game>
        {
            new("Game One Title", "First game description", 29.99m, GameGenre.Action, Tomorrow),
            new("Game Two Title", "Second game description", 59.99m, GameGenre.RPG, Tomorrow)
        };
        _query.ExecuteAsync(Arg.Any<Guid>(), 1, 20, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(IEnumerable<Game>, int)>((games, 2)));

        var response = await _handler.Handle(new Request(Guid.NewGuid(), 1, 20), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Library);
        Assert.Equal(2, response.Library.Total);
        Assert.Equal(1, response.Library.Page);
        Assert.Equal(20, response.Library.PageSize);
        Assert.Equal(2, response.Library.Games.Count());
    }

    [Fact]
    public async Task Handle_WhenLibraryIsEmpty_ShouldReturnEmptyPagedLibrary()
    {
        _query.ExecuteAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(IEnumerable<Game>, int)>((new List<Game>(), 0)));

        var response = await _handler.Handle(new Request(Guid.NewGuid(), 1, 20), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Library);
        Assert.Equal(0, response.Library.Total);
        Assert.Empty(response.Library.Games);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldReturnBadRequestWithoutQuerying()
    {
        var response = await _handler.Handle(new Request(Guid.Empty, 0, 0), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        await _query.DidNotReceive().ExecuteAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQueryThrowsException_ShouldPropagate()
    {
        _query.ExecuteAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<(IEnumerable<Game>, int)>(new Exception("Unexpected error")));

        await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(new Request(Guid.NewGuid(), 1, 20), CancellationToken.None));
    }
}
