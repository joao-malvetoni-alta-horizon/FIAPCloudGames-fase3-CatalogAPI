using CatalogAPI.Domain.Contexts.Games.Exceptions;

namespace CatalogAPI.Domain.Contexts.Games.ValueObjects;

public record GameTitle
{
    public const int MaxLength = 200;

    public string Value { get; init; }

    public GameTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidGameTitleException("Title cannot be null or empty.");

        if (value.Length > MaxLength)
            throw new InvalidGameTitleException($"Title cannot exceed {MaxLength} characters.");

        Value = value.Trim();
    }

    private GameTitle() { Value = null!; }
}
