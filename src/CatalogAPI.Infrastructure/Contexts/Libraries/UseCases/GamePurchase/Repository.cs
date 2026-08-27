using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Libraries.Commands;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Contexts.Libraries.UseCases.GamePurchase;

public class Repository(AppDbContext context) : IGamePurchase
{
    public async Task<Game> ExecuteAsync(Guid userId, Guid gameId, CancellationToken cancellationToken)
    {
        var game = await context.Games
            .FirstOrDefaultAsync(x => x.Id == gameId, cancellationToken)
            ?? throw new GameNotFoundException("Game not found in the catalog.");

        var alreadyOwns = await context.LibraryItems
            .AnyAsync(x => x.UserId == userId && x.GameId == gameId, cancellationToken);

        // Idempotency: the consumer may receive the same message more than once.
        if (alreadyOwns)
            return game;

        var item = new LibraryItem(userId, gameId);

        await context.LibraryItems.AddAsync(item, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return game;
    }
}
