using CatalogAPI.Domain.Contexts.Libraries.Entities;

namespace CatalogAPI.Domain.Tests.Libraries.Entities;

public class LibraryItemTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateLibraryItem()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var item = new LibraryItem(userId, gameId);

        Assert.Equal(userId, item.UserId);
        Assert.Equal(gameId, item.GameId);
        Assert.NotEqual(Guid.Empty, item.Id);
    }

    [Fact]
    public void Constructor_ShouldSetAcquiredOnToUtcNow()
    {
        var before = DateTime.UtcNow;

        var item = new LibraryItem(Guid.NewGuid(), Guid.NewGuid());

        var after = DateTime.UtcNow;
        Assert.InRange(item.AcquiredOn, before, after);
        Assert.Equal(DateTimeKind.Utc, item.AcquiredOn.Kind);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        var first = new LibraryItem(Guid.NewGuid(), Guid.NewGuid());
        var second = new LibraryItem(Guid.NewGuid(), Guid.NewGuid());

        Assert.NotEqual(first.Id, second.Id);
    }
}