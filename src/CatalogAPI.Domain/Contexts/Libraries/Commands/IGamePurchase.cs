using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Entities;

namespace CatalogAPI.Domain.Contexts.Libraries.Commands;

public interface IGamePurchase
{
    Task<Game> ExecuteAsync(Guid userId, Guid gameId, CancellationToken cancellationToken);
}
