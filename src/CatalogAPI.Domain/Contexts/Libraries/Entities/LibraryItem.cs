using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Libraries.Entities;

public class LibraryItem : Entity
{
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime AcquiredOn { get; private set; }

    protected LibraryItem() { }
    
    public LibraryItem(Guid userId, Guid gameId)
    {
        UserId = userId;
        GameId = gameId;
        AcquiredOn = DateTime.UtcNow;
    }

    public virtual Game Game { get; private set; } = null!;
}
