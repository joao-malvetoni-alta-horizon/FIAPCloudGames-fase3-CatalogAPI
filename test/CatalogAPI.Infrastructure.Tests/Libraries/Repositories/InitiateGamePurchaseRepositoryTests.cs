using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Infrastructure.Contexts.Libraries.UseCases.InitiateGamePurchase;
using CatalogAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Tests.Libraries.Repositories;

[Collection("Database")]
public class InitiateGamePurchaseRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public InitiateGamePurchaseRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

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
    public async Task ExecuteAsync_WhenGameExistsAndNotOwned_ShouldReturnGameAndFalse()
    {
        var game = await SeedGameAsync();

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (resultGame, alreadyOwns) = await repo.ExecuteAsync(
            new LibraryItem(Guid.NewGuid(), game.Id), CancellationToken.None);

        Assert.NotNull(resultGame);
        Assert.Equal(game.Id, resultGame.Id);
        Assert.False(alreadyOwns);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameAlreadyOwned_ShouldReturnGameAndTrue()
    {
        var game = await SeedGameAsync();
        var userId = Guid.NewGuid();

        using (var seed = _fixture.CreateContext())
        {
            seed.LibraryItems.Add(new LibraryItem(userId, game.Id));
            await seed.SaveChangesAsync();
        }

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (resultGame, alreadyOwns) = await repo.ExecuteAsync(
            new LibraryItem(userId, game.Id), CancellationToken.None);

        Assert.NotNull(resultGame);
        Assert.True(alreadyOwns);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameDoesNotExist_ShouldReturnNullAndFalse()
    {
        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (resultGame, alreadyOwns) = await repo.ExecuteAsync(
            new LibraryItem(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Null(resultGame);
        Assert.False(alreadyOwns);
    }
}
