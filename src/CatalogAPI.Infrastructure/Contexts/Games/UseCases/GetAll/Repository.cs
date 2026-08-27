using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Queries;
using CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Contexts.Games.UseCases.GetAll;

public class Repository(AppDbContext context) : IGetAll
{
    public async Task<(IReadOnlyCollection<Game>, int Total)> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var total = await context.Games.CountAsync(cancellationToken);
        var games = await context.Games
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (games, total);
    }
}
