namespace CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary.DTOs;

public record PagedLibraryResponse(
    IEnumerable<LibraryGameItemResponse> Games,
    int Page,
    int PageSize,
    int Total);
