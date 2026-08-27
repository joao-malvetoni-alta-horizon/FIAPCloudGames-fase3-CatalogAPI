using CatalogAPI.Domain.Contexts.Games.Entities;

namespace CatalogAPI.Domain.Contexts.Libraries.Queries;

public interface IGetLibrary
{
    Task<(IEnumerable<Game>, int Total)> ExecuteAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
