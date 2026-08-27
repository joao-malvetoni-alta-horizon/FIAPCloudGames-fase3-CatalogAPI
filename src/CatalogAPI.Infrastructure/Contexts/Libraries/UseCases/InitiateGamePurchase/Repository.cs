using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Queries;
using CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Contexts.Libraries.UseCases.InitiateGamePurchase;

public class Repository(AppDbContext context) : IGetGameAndCheckOwnership
{
    public async Task<(Game? Game, bool AlreadyOwns)> ExecuteAsync(LibraryItem item, CancellationToken cancellationToken)
    {
        var game = await context.Games.FirstOrDefaultAsync(x => x.Id == item.GameId, cancellationToken);
        
        if (game is null)
            return (null, false);
        
        var alreadyOwns = await context.LibraryItems
            .AnyAsync(x => x.UserId == item.UserId && x.GameId == item.GameId, cancellationToken);
        
        return (game, alreadyOwns);
    }
}
