namespace CatalogAPI.Domain.Contexts.Games.Commands;

public interface IDelete
{
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
