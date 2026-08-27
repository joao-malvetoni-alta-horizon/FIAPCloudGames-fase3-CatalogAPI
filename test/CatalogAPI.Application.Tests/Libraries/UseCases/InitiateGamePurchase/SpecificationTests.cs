using CatalogAPI.Application.Contexts.Libraries.UseCases.InitiateGamePurchase;

namespace CatalogAPI.Application.Tests.Libraries.UseCases.InitiateGamePurchase;

public class SpecificationTests
{
    [Fact]
    public void Ensure_WithValidRequest_ShouldReturnValidContract()
    {
        var contract = Specification.Ensure(new Request(Guid.NewGuid(), Guid.NewGuid()));

        Assert.True(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithEmptyUserId_ShouldReturnInvalidContract()
    {
        var contract = Specification.Ensure(new Request(Guid.Empty, Guid.NewGuid()));

        Assert.False(contract.IsValid);
    }

    [Fact]
    public void Ensure_WithEmptyGameId_ShouldReturnInvalidContract()
    {
        var contract = Specification.Ensure(new Request(Guid.NewGuid(), Guid.Empty));

        Assert.False(contract.IsValid);
    }
}