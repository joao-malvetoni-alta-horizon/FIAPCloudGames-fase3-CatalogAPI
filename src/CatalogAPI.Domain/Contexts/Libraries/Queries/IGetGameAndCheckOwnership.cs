using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Entities;

namespace CatalogAPI.Domain.Contexts.Libraries.Queries;

public interface IGetGameAndCheckOwnership
{
    Task<(Game? Game, bool AlreadyOwns)> ExecuteAsync(LibraryItem item, CancellationToken cancellationToken);
    
}
