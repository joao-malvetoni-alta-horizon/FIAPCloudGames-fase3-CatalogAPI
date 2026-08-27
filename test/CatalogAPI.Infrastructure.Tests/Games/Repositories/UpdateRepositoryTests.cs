using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Infrastructure.Contexts.Games.UseCases.Update;
using CatalogAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Tests.Games.Repositories;

[Collection("Database")]
public class UpdateRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public UpdateRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var context = _fixture.CreateContext();
        await context.Games.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Game> SeedGameAsync()
    {
        var game = new Game("Original Title", "Original description for game", 19.99m, GameGenre.Action, Tomorrow);
        using var context = _fixture.CreateContext();
        context.Games.Add(game);
        await context.SaveChangesAsync();
        return game;
    }

    [Fact]
    public async Task UpdateAsync_WithExistingGame_ShouldReturnTrue()
    {
        var seeded = await SeedGameAsync();

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var result = await repo.UpdateAsync(seeded.Id, "Updated Title", null, null, null, null, null, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ShouldReturnFalse()
    {
        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var result = await repo.UpdateAsync(Guid.NewGuid(), "New Title", null, null, null, null, null, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChangesToDatabase()
    {
        var seeded = await SeedGameAsync();
        var newTitle = "Updated Game Title";
        var newPrice = 99.99m;
        var newGenre = GameGenre.RPG;

        using var writeContext = _fixture.CreateContext();
        var repo = new Repository(writeContext);
        await repo.UpdateAsync(seeded.Id, newTitle, null, newPrice, newGenre, null, null, CancellationToken.None);

        using var readContext = _fixture.CreateContext();
        var updated = await readContext.Games.FindAsync(seeded.Id);

        Assert.NotNull(updated);
        Assert.Equal(newTitle, updated.Title.Value);
        Assert.Equal(newPrice, updated.Price.Amount);
        Assert.Equal(newGenre, updated.Genre);
    }

    [Fact]
    public async Task UpdateAsync_WithNullFields_ShouldNotOverwriteExistingValues()
    {
        var seeded = await SeedGameAsync();

        using var writeContext = _fixture.CreateContext();
        var repo = new Repository(writeContext);
        await repo.UpdateAsync(seeded.Id, null, null, null, null, null, null, CancellationToken.None);

        using var readContext = _fixture.CreateContext();
        var updated = await readContext.Games.FindAsync(seeded.Id);

        Assert.NotNull(updated);
        Assert.Equal(seeded.Title.Value, updated.Title.Value);
        Assert.Equal(seeded.Price.Amount, updated.Price.Amount);
        Assert.Equal(seeded.Description, updated.Description);
        Assert.Equal(seeded.Genre, updated.Genre);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetUpdatedAtTimestamp()
    {
        var seeded = await SeedGameAsync();

        using var writeContext = _fixture.CreateContext();
        var repo = new Repository(writeContext);
        await repo.UpdateAsync(seeded.Id, "Updated Title", null, null, null, null, null, CancellationToken.None);

        using var readContext = _fixture.CreateContext();
        var updated = await readContext.Games.FindAsync(seeded.Id);

        Assert.NotNull(updated);
        Assert.NotNull(updated.UpdatedAt);
    }
}