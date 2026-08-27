using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Infrastructure.Data;

namespace CatalogAPI.Infrastructure.Contexts.Games.UseCases.Create;

public class Repository(AppDbContext context) : ICreate
{
    public async Task<Game> CreateAsync(Game game, CancellationToken cancellationToken)
    {
        await context.Games.AddAsync(game, cancellationToken);
        await  context.SaveChangesAsync(cancellationToken);
        return game;
    }
}