using CatalogAPI.Application.Contexts.Games.UseCases.GetAll;

namespace CatalogAPI.Application.Tests.Games.UseCases.GetAll;

public class SpecificationTests
{
    [Fact]
    public void Ensure_WithValidRequest_ShouldReturnValidContract()
    {
        var contract = Specification.Ensure(new Request(1, 20));

        Assert.True(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithZeroPage_ShouldReturnInvalidContract()
    {
        var contract = Specification.Ensure(new Request(0, 20));

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithZeroPageSize_ShouldReturnInvalidContract()
    {
        var contract = Specification.Ensure(new Request(1, 0));

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithPageSizeExceeding50_ShouldReturnInvalidContract()
    {
        var contract = Specification.Ensure(new Request(1, 51));

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithPageSizeOf50_ShouldReturnValidContract()
    {
        var contract = Specification.Ensure(new Request(1, 50));

        Assert.True(contract.IsValid);
    }
}