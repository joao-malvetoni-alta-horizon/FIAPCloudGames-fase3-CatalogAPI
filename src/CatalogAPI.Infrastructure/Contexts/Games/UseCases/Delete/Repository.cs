using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Contexts.Games.UseCases.Delete;

public class Repository(AppDbContext context) : IDelete
{
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rows = await context.Games
            .Where(g => g.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return rows > 0;
    }
}