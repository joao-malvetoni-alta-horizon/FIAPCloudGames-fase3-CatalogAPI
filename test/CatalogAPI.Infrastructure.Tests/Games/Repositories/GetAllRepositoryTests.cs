using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Infrastructure.Contexts.Games.UseCases.GetAll;
using CatalogAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Tests.Games.Repositories;

[Collection("Database")]
public class GetAllRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public GetAllRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var context = _fixture.CreateContext();
        await context.Games.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedGamesAsync(int count)
    {
        using var context = _fixture.CreateContext();
        for (var i = 1; i <= count; i++)
            context.Games.Add(new Game($"Game Title Number {i}", $"Description for game number {i}", i * 10m, GameGenre.Action, Tomorrow));
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_WhenDatabaseIsEmpty_ShouldReturnEmptyCollection()
    {
        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (games, total) = await repo.GetAllAsync(1, 20, CancellationToken.None);

        Assert.Empty(games);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task GetAllAsync_WhenDatabaseHasGames_ShouldReturnAllGames()
    {
        await SeedGamesAsync(3);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (games, total) = await repo.GetAllAsync(1, 20, CancellationToken.None);

        Assert.Equal(3, total);
        Assert.Equal(3, games.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCorrectTotalRegardlessOfPageSize()
    {
        await SeedGamesAsync(5);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (games, total) = await repo.GetAllAsync(1, 2, CancellationToken.None);

        Assert.Equal(5, total);
        Assert.Equal(2, games.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldSkipFirstPageAndReturnSecond()
    {
        await SeedGamesAsync(5);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (page1Games, _) = await repo.GetAllAsync(1, 3, CancellationToken.None);
        var (page2Games, _) = await repo.GetAllAsync(2, 3, CancellationToken.None);

        Assert.Equal(3, page1Games.Count);
        Assert.Equal(2, page2Games.Count);
        Assert.Empty(page1Games.Select(g => g.Id).Intersect(page2Games.Select(g => g.Id)));
    }

    [Fact]
    public async Task GetAllAsync_LastPage_ShouldReturnRemainingGames()
    {
        await SeedGamesAsync(7);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (games, total) = await repo.GetAllAsync(3, 3, CancellationToken.None);

        Assert.Equal(7, total);
        Assert.Single(games);
    }
}