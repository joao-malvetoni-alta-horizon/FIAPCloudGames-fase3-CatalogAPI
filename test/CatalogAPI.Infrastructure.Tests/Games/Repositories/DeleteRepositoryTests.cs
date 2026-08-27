using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Infrastructure.Contexts.Games.UseCases.Delete;
using CatalogAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Tests.Games.Repositories;

[Collection("Database")]
public class DeleteRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public DeleteRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var context = _fixture.CreateContext();
        await context.Games.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Game> SeedGameAsync()
    {
        var game = new Game("Game Title Test", "A description for the game", 49.99m, GameGenre.Puzzle, Tomorrow);
        using var context = _fixture.CreateContext();
        context.Games.Add(game);
        await context.SaveChangesAsync();
        return game;
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldReturnTrue()
    {
        var seeded = await SeedGameAsync();

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var result = await repo.DeleteAsync(seeded.Id, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var result = await repo.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveGameFromDatabase()
    {
        var seeded = await SeedGameAsync();

        using var writeContext = _fixture.CreateContext();
        var repo = new Repository(writeContext);
        await repo.DeleteAsync(seeded.Id, CancellationToken.None);

        using var readContext = _fixture.CreateContext();
        var deleted = await readContext.Games.FindAsync(seeded.Id);

        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ShouldOnlyRemoveTargetGame()
    {
        var target = await SeedGameAsync();
        var other = new Game("Other Game Title", "Description for the other game", 9.99m, GameGenre.Sports, Tomorrow);

        using var seedContext = _fixture.CreateContext();
        seedContext.Games.Add(other);
        await seedContext.SaveChangesAsync();

        using var writeContext = _fixture.CreateContext();
        var repo = new Repository(writeContext);
        await repo.DeleteAsync(target.Id, CancellationToken.None);

        using var readContext = _fixture.CreateContext();
        var remaining = await readContext.Games.FindAsync(other.Id);

        Assert.NotNull(remaining);
        Assert.Equal(other.Id, remaining.Id);
    }
}