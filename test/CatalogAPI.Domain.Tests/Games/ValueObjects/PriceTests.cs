using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Games.ValueObjects;

namespace CatalogAPI.Domain.Tests.Games.ValueObjects;

public class PriceTests
{
    [Fact]
    public void Constructor_WithPositiveAmount_ShouldCreatePrice()
    {
        var price = new Price(9.99m);

        Assert.Equal(9.99m, price.Amount);
    }

    [Fact]
    public void Constructor_WithZeroAmount_ShouldCreatePrice()
    {
        var price = new Price(0m);

        Assert.Equal(0m, price.Amount);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowInvalidPriceException()
    {
        Assert.Throws<InvalidPriceException>(() => new Price(-0.01m));
    }

    [Fact]
    public void TwoPrices_WithSameAmount_ShouldBeEqual()
    {
        var price1 = new Price(29.99m);
        var price2 = new Price(29.99m);

        Assert.Equal(price1, price2);
    }

    [Fact]
    public void TwoPrices_WithDifferentAmounts_ShouldNotBeEqual()
    {
        var price1 = new Price(10m);
        var price2 = new Price(20m);

        Assert.NotEqual(price1, price2);
    }
}