using CatalogAPI.Application.Contexts.Games.UseCases.GetAll;
using CatalogAPI.Application.Contexts.Games.UseCases.GetAll.DTOs;
using CatalogAPI.Application.Shared.Cache;
using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.Queries;
using NSubstitute;

namespace CatalogAPI.Application.Tests.Games.UseCases.GetAll;

public class HandlerTests
{
    private readonly IGetAll _repository;
    private readonly ICacheService _cacheService;
    private readonly Handler _handler;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public HandlerTests()
    {
        _repository = Substitute.For<IGetAll>();
        _cacheService = Substitute.For<ICacheService>();

        // Cache sempre "frio": delega para a factory, mantendo os testes focados no handler.
        _cacheService.GetOrSetAsync(
                Arg.Any<string>(), Arg.Any<Func<Task<PagedGameResponse?>>>(),
                Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<Task<PagedGameResponse?>>>()());

        _handler = new Handler(_repository, _cacheService);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnPagedResponse()
    {
        var games = new List<Game>
        {
            new("Game One Title", "First game description", 29.99m, GameGenre.Action, Tomorrow),
            new("Game Two Title", "Second game description", 59.99m, GameGenre.RPG, Tomorrow)
        };

        _repository.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(IReadOnlyCollection<Game>, int)>((games.AsReadOnly(), 2)));

        var response = await _handler.Handle(new Request(1, 20), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Total);
        Assert.Equal(1, response.Data.Page);
        Assert.Equal(20, response.Data.PageSize);
        Assert.Equal(2, response.Data.Games.Count());
    }

    [Fact]
    public async Task Handle_WithEmptyGameList_ShouldReturnEmptyPagedResponse()
    {
        _repository.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(IReadOnlyCollection<Game>, int)>((new List<Game>().AsReadOnly(), 0)));

        var response = await _handler.Handle(new Request(1, 20), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Data);
        Assert.Equal(0, response.Data.Total);
        Assert.Empty(response.Data.Games);
    }

    [Fact]
    public async Task Handle_WithInvalidPagination_ShouldReturnBadRequestWithoutCallingRepository()
    {
        var response = await _handler.Handle(new Request(0, 0), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        await _repository.DidNotReceive().GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagate()
    {
        _repository.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<(IReadOnlyCollection<Game>, int)>(new Exception("Unexpected error")));

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new Request(1, 20), CancellationToken.None));
    }
}