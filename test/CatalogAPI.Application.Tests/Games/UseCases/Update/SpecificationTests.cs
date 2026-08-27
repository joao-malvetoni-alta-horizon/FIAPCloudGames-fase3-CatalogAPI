using CatalogAPI.Application.Contexts.Games.UseCases.Update;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.ValueObjects;

namespace CatalogAPI.Application.Tests.Games.UseCases.Update;

public class SpecificationTests
{
    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    private static Request ValidRequest() =>
        new(Guid.NewGuid(), "Updated Title", "Updated description text", 59.99m, GameGenre.RPG, GameStatus.Active, Tomorrow);

    [Fact]
    public void Ensure_WithValidFullRequest_ShouldReturnValidContract()
    {
        var contract = Specification.Ensure(ValidRequest());

        Assert.True(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithAllNullOptionalFields_ShouldReturnValidContract()
    {
        var request = new Request(Guid.NewGuid(), null, null, null, null, null, null);

        var contract = Specification.Ensure(request);

        Assert.True(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithEmptyGuid_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Id = Guid.Empty };

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithTitleShorterThan3Characters_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Title = "AB" }; // 2 chars

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithTitleExceedingMaxLength_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Title = new string('A', GameTitle.MaxLength + 1) };

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithDescriptionShorterThan10Characters_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Description = "Too short" }; // 9 chars

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithDescriptionExceeding500Characters_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { Description = new string('A', 501) };

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
    public void Ensure_WithReleaseDateBefore1950_ShouldReturnInvalidContract()
    {
        var request = ValidRequest() with { ReleaseDate = new DateOnly(1949, 12, 31) };

        var contract = Specification.Ensure(request);

        Assert.False(contract.IsValid);
    }
}