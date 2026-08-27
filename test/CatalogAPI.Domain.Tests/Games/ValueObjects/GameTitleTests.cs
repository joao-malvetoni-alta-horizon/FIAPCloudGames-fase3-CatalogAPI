using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Games.ValueObjects;

namespace CatalogAPI.Domain.Tests.Games.ValueObjects;

public class GameTitleTests
{
    [Fact]
    public void Constructor_WithValidTitle_ShouldCreateGameTitle()
    {
        var title = new GameTitle("Valid Title");

        Assert.Equal("Valid Title", title.Value);
    }

    [Fact]
    public void Constructor_WithLeadingAndTrailingWhitespace_ShouldTrimTitle()
    {
        var title = new GameTitle("  Trimmed Title  ");

        Assert.Equal("Trimmed Title", title.Value);
    }

    [Fact]
    public void Constructor_WithExactMaxLength_ShouldCreateGameTitle()
    {
        var value = new string('A', GameTitle.MaxLength);

        var title = new GameTitle(value);

        Assert.Equal(GameTitle.MaxLength, title.Value.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespace_ShouldThrowInvalidGameTitleException(string? value)
    {
        Assert.Throws<InvalidGameTitleException>(() => new GameTitle(value!));
    }

    [Fact]
    public void Constructor_WithTitleExceedingMaxLength_ShouldThrowInvalidGameTitleException()
    {
        var longTitle = new string('A', GameTitle.MaxLength + 1);

        Assert.Throws<InvalidGameTitleException>(() => new GameTitle(longTitle));
    }

    [Fact]
    public void TwoGameTitles_WithSameValue_ShouldBeEqual()
    {
        var title1 = new GameTitle("Same Title");
        var title2 = new GameTitle("Same Title");

        Assert.Equal(title1, title2);
    }

    [Fact]
    public void TwoGameTitles_WithDifferentValues_ShouldNotBeEqual()
    {
        var title1 = new GameTitle("Title One");
        var title2 = new GameTitle("Title Two");

        Assert.NotEqual(title1, title2);
    }
}