using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> context)
    : DbContext(context)
{
    public DbSet<Game> Games { get; set; }
    public DbSet<LibraryItem> LibraryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}