using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Games.ValueObjects;
using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Games.Entities;

public class Game : Entity
{
    #region Properties

    public GameTitle Title { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public Price Price { get; private set; } = null!;
    public GameGenre Genre { get; private set; }
    public GameStatus Status { get; private set; }
    public DateOnly ReleaseDate { get; private set; }
    
    private List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    #endregion

    #region Constructors

    private Game() { }

    public Game(
        string title,
        string description,
        decimal price,
        GameGenre genre,
        DateOnly releaseDate)
    {
        if (releaseDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidReleaseDateException("Invalid release date.");

        if (description?.Length > 2000)
            throw new DomainValidationException("Description cannot exceed 2000 characters.");

        Title = new GameTitle(title);
        Description = description ?? string.Empty;
        Price = new Price(price);
        Genre = genre;
        Status = GameStatus.Active;
        ReleaseDate = releaseDate;
    }

    #endregion

    #region Methods

    public void Update(
        string? title = null,
        string? description = null,
        decimal? price = null,
        GameGenre? genre = null,
        DateOnly? releaseDate = null,
        GameStatus? status = null)
    {
        if (title is not null)
            Title = new GameTitle(title);

        if (description is not null)
        {
            if (description.Length > 2000)
                throw new DomainValidationException("Description cannot exceed 2000 characters.");
            Description = description;
        }

        if (price.HasValue)
            Price = new Price(price.Value);

        if (genre.HasValue)
            Genre = genre.Value;

        if (releaseDate.HasValue)
            ReleaseDate = releaseDate.Value;

        if (status.HasValue)
            Status = status.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = GameStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    #endregion
    
}
