using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.Exceptions;

namespace CatalogAPI.Domain.Tests.Games.Entities;

public class GameTests
{
    private static DateOnly Tomorrow => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
    private static DateOnly Yesterday => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

    #region Constructor

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateGame()
    {
        var game = new Game("Valid Title", "A description", 49.99m, GameGenre.Action, Tomorrow);

        Assert.Equal("Valid Title", game.Title.Value);
        Assert.Equal("A description", game.Description);
        Assert.Equal(49.99m, game.Price.Amount);
        Assert.Equal(GameGenre.Action, game.Genre);
        Assert.Equal(Tomorrow, game.ReleaseDate);
        Assert.Equal(GameStatus.Active, game.Status);
        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.True(game.CreatedAt <= DateTime.UtcNow);
        Assert.Null(game.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithTodayReleaseDate_ShouldCreateGame()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.RPG, Today);

        Assert.Equal(Today, game.ReleaseDate);
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldUseEmptyString()
    {
        var game = new Game("Title", null!, 10m, GameGenre.RPG, Tomorrow);

        Assert.Equal(string.Empty, game.Description);
    }

    [Fact]
    public void Constructor_WithDescriptionOf2000Characters_ShouldCreateGame()
    {
        var maxDescription = new string('A', 2000);

        var game = new Game("Title", maxDescription, 10m, GameGenre.Action, Tomorrow);

        Assert.Equal(maxDescription, game.Description);
    }

    [Fact]
    public void Constructor_ShouldSetStatusToActive()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        Assert.Equal(GameStatus.Active, game.Status);
    }

    [Fact]
    public void Constructor_ShouldInitializeDomainEventsAsEmpty()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        Assert.Empty(game.DomainEvents);
    }

    [Fact]
    public void Constructor_WithPastReleaseDate_ShouldThrowInvalidReleaseDateException()
    {
        Assert.Throws<InvalidReleaseDateException>(() =>
            new Game("Title", "Description", 10m, GameGenre.Action, Yesterday));
    }

    [Fact]
    public void Constructor_WithDescriptionExceeding2000Characters_ShouldThrowDomainValidationException()
    {
        var longDescription = new string('A', 2001);

        Assert.Throws<DomainValidationException>(() =>
            new Game("Title", longDescription, 10m, GameGenre.Action, Tomorrow));
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WithNewTitle_ShouldUpdateTitle()
    {
        var game = new Game("Old Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Update(title: "New Title");

        Assert.Equal("New Title", game.Title.Value);
    }

    [Fact]
    public void Update_WithNewDescription_ShouldUpdateDescription()
    {
        var game = new Game("Title", "Old Description", 10m, GameGenre.Action, Tomorrow);

        game.Update(description: "New Description");

        Assert.Equal("New Description", game.Description);
    }

    [Fact]
    public void Update_WithNewPrice_ShouldUpdatePrice()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Update(price: 59.99m);

        Assert.Equal(59.99m, game.Price.Amount);
    }

    [Fact]
    public void Update_WithNewGenre_ShouldUpdateGenre()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Update(genre: GameGenre.RPG);

        Assert.Equal(GameGenre.RPG, game.Genre);
    }

    [Fact]
    public void Update_WithNewReleaseDate_ShouldUpdateReleaseDate()
    {
        var newDate = Tomorrow.AddDays(10);
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Update(releaseDate: newDate);

        Assert.Equal(newDate, game.ReleaseDate);
    }

    [Fact]
    public void Update_WithNewStatus_ShouldUpdateStatus()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Update(status: GameStatus.ComingSoon);

        Assert.Equal(GameStatus.ComingSoon, game.Status);
    }

    [Fact]
    public void Update_ShouldSetUpdatedAt()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Update(title: "New Title");

        Assert.NotNull(game.UpdatedAt);
        Assert.True(game.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Update_WithAllNullParameters_ShouldNotChangeFieldValues()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Update();

        Assert.Equal("Title", game.Title.Value);
        Assert.Equal("Description", game.Description);
        Assert.Equal(10m, game.Price.Amount);
        Assert.Equal(GameGenre.Action, game.Genre);
        Assert.Equal(Tomorrow, game.ReleaseDate);
        Assert.Equal(GameStatus.Active, game.Status);
    }

    [Fact]
    public void Update_WithDescriptionExceeding2000Characters_ShouldThrowDomainValidationException()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);
        var longDescription = new string('A', 2001);

        Assert.Throws<DomainValidationException>(() => game.Update(description: longDescription));
    }

    #endregion

    #region Deactivate

    [Fact]
    public void Deactivate_ShouldSetStatusToInactive()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Deactivate();

        Assert.Equal(GameStatus.Inactive, game.Status);
    }

    [Fact]
    public void Deactivate_ShouldSetUpdatedAt()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.Deactivate();

        Assert.NotNull(game.UpdatedAt);
        Assert.True(game.UpdatedAt <= DateTime.UtcNow);
    }

    #endregion

    #region ClearDomainEvents

    [Fact]
    public void ClearDomainEvents_ShouldResultInEmptyDomainEvents()
    {
        var game = new Game("Title", "Description", 10m, GameGenre.Action, Tomorrow);

        game.ClearDomainEvents();

        Assert.Empty(game.DomainEvents);
    }

    #endregion
}