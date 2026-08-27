using CatalogAPI.Application.Contexts.Games.UseCases.Create;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.ValueObjects;

namespace CatalogAPI.Application.Tests.Games.UseCases.Create;

public class SpecificationTests
{
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    private static readonly DateOnly Yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

    private static Request ValidRequest() =>
        new("Game Title Test", "A valid description for the game", 49.99m, GameGenre.Action, Tomorrow);

    [Fact]
    public void Ensure_WithValidRequest_ShouldReturnValidContract()
    {
        var contract = Specification.Ensure(ValidRequest());

        Assert.True(contract.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ensure_WithNullOrEmptyTitle_ShouldReturnInvalidContract(string? title)
    {
        var request = ValidRequest() with { Title = title! };

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithTitleShorterThan11Characters_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Title = "ShortTitle" }; // 10 chars

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithTitleExceedingMaxLength_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Title = new string('A', GameTitle.MaxLength) }; // 200 chars, needs < 200

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ensure_WithNullOrEmptyDescription_ShouldReturnInvalidContract(string? description)
    {
        var request = ValidRequest() with { Description = description! };

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithDescriptionShorterThan11Characters_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Description = "Too short." }; // 10 chars

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithDescriptionOf2000Characters_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Description = new string('A', 2000) }; // needs < 2000

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithNegativePrice_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Price = -0.01m };

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithZeroPrice_ShouldReturnValidContract()
    {
        var request = ValidRequest() with { Price = 0m };

        var contract = Specification.Ensure(request);

        Assert.True(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithPastReleaseDate_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { ReleaseDate = Yesterday };

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }
}