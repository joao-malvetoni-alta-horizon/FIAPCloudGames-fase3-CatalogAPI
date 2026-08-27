using CatalogAPI.Application.Contexts.Games.UseCases.GetById;

namespace CatalogAPI.Application.Tests.Games.UseCases.GetById;

public class SpecificationTests
{
    [Fact]
    public void Ensure_WithValidId_ShouldReturnValidContract()
    {
        var contract = Specification.Ensure(new Request(Guid.NewGuid()));

        Assert.True(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithEmptyGuid_ShouldReturnInvalidContract()
    {
        var contract = Specification.Ensure(new Request(Guid.Empty));

        Assert.False(contract.IsValid);
    }
}