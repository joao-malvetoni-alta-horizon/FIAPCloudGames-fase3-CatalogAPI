namespace CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary.DTOs;

public record PaginationParametersRequest(int Page = 1, int PageSize = 20);
