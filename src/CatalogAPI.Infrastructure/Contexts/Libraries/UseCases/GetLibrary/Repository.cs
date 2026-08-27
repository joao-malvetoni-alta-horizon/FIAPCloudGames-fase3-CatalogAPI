using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Queries;
using CatalogAPI.Infrastructure.Data;

namespace CatalogAPI.Infrastructure.Contexts.Libraries.UseCases.GetLibrary;

public class Repository : IGetLibrary
{
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Game>, int Total)> ExecuteAsync(
        Guid userId, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken)
    {
        var query = _context.LibraryItems
            .AsNoTracking()
            .Where(libraryItem => libraryItem.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);
        
        if (totalCount == 0)
            return (Enumerable.Empty<Game>(), 0);
        
        var games = await query
            .Include(libraryItem => libraryItem.Game)
            .OrderBy(x => x.Game.Title.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(libraryItem => libraryItem.Game)
            .ToListAsync(cancellationToken);

        return (games, totalCount);
    }
}