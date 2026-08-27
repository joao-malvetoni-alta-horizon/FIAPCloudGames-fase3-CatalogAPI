using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Queries;
using CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Contexts.Games.UseCases.GetById;

public class Repository(AppDbContext context) : IGetById
{
    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}