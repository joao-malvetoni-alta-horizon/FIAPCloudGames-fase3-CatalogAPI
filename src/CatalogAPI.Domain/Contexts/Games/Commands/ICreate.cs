using CatalogAPI.Domain.Contexts.Games.Entities;

namespace CatalogAPI.Domain.Contexts.Games.Commands;

public interface ICreate
{
    Task<Game> CreateAsync(Game game, CancellationToken cancellationToken);
}