using CatalogAPI.Domain.Contexts.Games.Entities;

namespace CatalogAPI.Domain.Contexts.Games.Queries;

public interface IGetById
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
