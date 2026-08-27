using CatalogAPI.Domain.Contexts.Games.Enums;

namespace CatalogAPI.Domain.Contexts.Games.Commands;

public interface IUpdate
{
    Task<bool> UpdateAsync(
        Guid id,
        string? title,
        string? description,
        decimal? price,
        GameGenre? genre,
        GameStatus? status,
        DateOnly? releaseDate,
        CancellationToken cancellationToken);
}
