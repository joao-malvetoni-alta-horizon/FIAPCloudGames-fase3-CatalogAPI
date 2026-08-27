using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Infrastructure.Contexts.Games.UseCases.Create;
using CatalogAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Tests.Games.Repositories;

[Collection("Database")]
public class CreateRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public CreateRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var context = _fixture.CreateContext();
        await context.Games.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_ShouldPersistGameToDatabase()
    {
        var game = new Game("Game Title Test", "A description for the game", 49.99m, GameGenre.Action, Tomorrow);

        using var writeContext = _fixture.CreateContext();
        var repo = new Repository(writeContext);
        await repo.CreateAsync(game, CancellationToken.None);

        using var readContext = _fixture.CreateContext();
        var persisted = await readContext.Games.FindAsync(game.Id);

        Assert.NotNull(persisted);
        Assert.Equal(game.Title.Value, persisted.Title.Value);
        Assert.Equal(game.Price.Amount, persisted.Price.Amount);
        Assert.Equal(game.Description, persisted.Description);
        Assert.Equal(game.Genre, persisted.Genre);
        Assert.Equal(game.Status, persisted.Status);
        Assert.Equal(game.ReleaseDate, persisted.ReleaseDate);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedGame()
    {
        var game = new Game("Game Title Test", "A description for the game", 29.99m, GameGenre.RPG, Tomorrow);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);
        var result = await repo.CreateAsync(game, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(game.Id, result.Id);
        Assert.Equal(game.Title.Value, result.Title.Value);
        Assert.Equal(game.Price.Amount, result.Price.Amount);
        Assert.Equal(game.Genre, result.Genre);
    }

    [Fact]
    public async Task CreateAsync_TwoGames_ShouldGenerateDistinctIds()
    {
        var game1 = new Game("First Game Title", "Description for the first game", 10m, GameGenre.Action, Tomorrow);
        var game2 = new Game("Second Game Title", "Description for the second game", 20m, GameGenre.RPG, Tomorrow);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);
        await repo.CreateAsync(game1, CancellationToken.None);
        await repo.CreateAsync(game2, CancellationToken.None);

        Assert.NotEqual(game1.Id, game2.Id);
    }
}