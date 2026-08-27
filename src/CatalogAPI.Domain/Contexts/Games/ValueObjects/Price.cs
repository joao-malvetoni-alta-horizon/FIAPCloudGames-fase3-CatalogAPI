using CatalogAPI.Domain.Contexts.Games.Exceptions;

namespace CatalogAPI.Domain.Contexts.Games.ValueObjects;

public record Price
{
    public decimal Amount { get; init; }

    public Price(decimal amount)
    {
        if (amount < 0)
            throw new InvalidPriceException("Amount cannot be negative");

        Amount = amount;
    }

    private Price() { }
}
