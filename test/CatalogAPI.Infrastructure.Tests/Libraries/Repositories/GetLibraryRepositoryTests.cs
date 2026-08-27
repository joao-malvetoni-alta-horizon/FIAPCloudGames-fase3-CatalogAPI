using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Infrastructure.Contexts.Libraries.UseCases.GetLibrary;
using CatalogAPI.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Tests.Libraries.Repositories;

[Collection("Database")]
public class GetLibraryRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public GetLibraryRepositoryTests(DatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var context = _fixture.CreateContext();
        await context.LibraryItems.ExecuteDeleteAsync();
        await context.Games.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedOwnedGamesAsync(Guid userId, int count)
    {
        using var context = _fixture.CreateContext();
        for (var i = 1; i <= count; i++)
        {
            var game = new Game($"Game Title Number {i:D2}", $"Description {i}", i * 10m, GameGenre.Action, Tomorrow);
            context.Games.Add(game);
            context.LibraryItems.Add(new LibraryItem(userId, game.Id));
        }
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoGames_ShouldReturnEmptyCollection()
    {
        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (games, total) = await repo.ExecuteAsync(Guid.NewGuid(), 1, 20, CancellationToken.None);

        Assert.Empty(games);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserOwnsGames_ShouldReturnThem()
    {
        var userId = Guid.NewGuid();
        await SeedOwnedGamesAsync(userId, 3);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (games, total) = await repo.ExecuteAsync(userId, 1, 20, CancellationToken.None);

        Assert.Equal(3, total);
        Assert.Equal(3, games.Count());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOnlyTheGivenUsersGames()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        await SeedOwnedGamesAsync(userId, 2);
        await SeedOwnedGamesAsync(otherUserId, 4);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (games, total) = await repo.ExecuteAsync(userId, 1, 20, CancellationToken.None);

        Assert.Equal(2, total);
        Assert.Equal(2, games.Count());
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ShouldSkipFirstPageAndReturnSecond()
    {
        var userId = Guid.NewGuid();
        await SeedOwnedGamesAsync(userId, 5);

        using var context = _fixture.CreateContext();
        var repo = new Repository(context);

        var (page1, total) = await repo.ExecuteAsync(userId, 1, 3, CancellationToken.None);
        var (page2, _) = await repo.ExecuteAsync(userId, 2, 3, CancellationToken.None);

        Assert.Equal(5, total);
        Assert.Equal(3, page1.Count());
        Assert.Equal(2, page2.Count());
        Assert.Empty(page1.Select(g => g.Id).Intersect(page2.Select(g => g.Id)));
    }
}
