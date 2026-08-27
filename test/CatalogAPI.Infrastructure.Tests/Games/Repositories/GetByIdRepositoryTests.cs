using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Infrastructure.Contexts.Games.UseCases.GetById;
using CatalogAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Tests.Games.Repositories;

[Collection("Database")]
public class GetByIdRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public GetByIdRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var context = _fixture.CreateContext();
        await context.Games.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Game> SeedGameAsync()
    {
        var game = new Game("Game Title Test", "A description for the game", 49.99m, GameGenre.Strategy, Tomorrow);
        using var context = _fixture.CreateContext();
        context.Games.Add(game);
        await context.SaveChangesAsync();
        return game;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCorrectGame()
    {
        var seeded = await SeedGameAsync();

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var result = await repo.GetByIdAsync(seeded.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(seeded.Id, result.Id);
        Assert.Equal(seeded.Title.Value, result.Title.Value);
        Assert.Equal(seeded.Price.Amount, result.Price.Amount);
        Assert.Equal(seeded.Description, result.Description);
        Assert.Equal(seeded.Genre, result.Genre);
        Assert.Equal(seeded.Status, result.Status);
        Assert.Equal(seeded.ReleaseDate, result.ReleaseDate);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var result = await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }
}