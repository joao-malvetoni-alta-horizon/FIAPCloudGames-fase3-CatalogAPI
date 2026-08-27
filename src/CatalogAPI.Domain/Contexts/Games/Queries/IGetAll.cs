using CatalogAPI.Domain.Contexts.Games.Entities;

namespace CatalogAPI.Domain.Contexts.Games.Queries;

public interface IGetAll
{
    Task<(IReadOnlyCollection<Game>, int Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
}
