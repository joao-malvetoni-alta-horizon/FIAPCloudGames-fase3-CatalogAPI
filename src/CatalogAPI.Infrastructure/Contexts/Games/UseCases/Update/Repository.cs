using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Contexts.Games.UseCases.Update;

public class Repository(AppDbContext context) : IUpdate
{
    public async Task<bool> UpdateAsync(
        Guid id,
        string? title,
        string? description,
        decimal? price,
        GameGenre? genre,
        GameStatus? status,
        DateOnly? releaseDate,
        CancellationToken cancellationToken = default)
    {
        var game = await context.Games.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        
        if (game is null)
            return false;
        
        game.Update(title, description, price, genre, releaseDate, status);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}