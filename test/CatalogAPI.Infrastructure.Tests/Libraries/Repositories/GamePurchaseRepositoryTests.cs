using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Infrastructure.Contexts.Libraries.UseCases.GamePurchase;
using CatalogAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Tests.Libraries.Repositories;

[Collection("Database")]
public class GamePurchaseRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public GamePurchaseRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var context = _fixture.CreateContext();
        await context.LibraryItems.ExecuteDeleteAsync();
        await context.Games.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Game> SeedGameAsync()
    {
        using var context = _fixture.CreateContext();
        var game = new Game("Game Title Test", "A description for the game", 49.99m, GameGenre.Action, Tomorrow);
        context.Games.Add(game);
        await context.SaveChangesAsync();
        return game;
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameExistsAndNotOwned_ShouldAddLibraryItemAndReturnGame()
    {
        var game = await SeedGameAsync();
        var userId = Guid.NewGuid();

        using (var context = _fixture.CreateContext())
        {
            var repo = new Repository(context);
            var result = await repo.ExecuteAsync(userId, game.Id, CancellationToken.None);
            Assert.Equal(game.Id, result.Id);
        }

        using var verify = _fixture.CreateContext();
        Assert.True(await verify.LibraryItems.AnyAsync(x => x.UserId == userId && x.GameId == game.Id));
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserAlreadyOwnsGame_ShouldBeIdempotentAndNotInsertDuplicate()
    {
        var game = await SeedGameAsync();
        var userId = Guid.NewGuid();

        using (var seed = _fixture.CreateContext())
        {
            seed.LibraryItems.Add(new LibraryItem(userId, game.Id));
            await seed.SaveChangesAsync();
        }

        using (var context = _fixture.CreateContext())
        {
            var repo = new Repository(context);
            var result = await repo.ExecuteAsync(userId, game.Id, CancellationToken.None);
            Assert.Equal(game.Id, result.Id);
        }

        using var verify = _fixture.CreateContext();
        var count = await verify.LibraryItems.CountAsync(x => x.UserId == userId && x.GameId == game.Id);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameDoesNotExist_ShouldThrowGameNotFoundException()
    {
        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        await Assert.ThrowsAsync<GameNotFoundException>(() =>
            repo.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }
}
