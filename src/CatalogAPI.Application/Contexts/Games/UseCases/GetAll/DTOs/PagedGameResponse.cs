namespace CatalogAPI.Application.Contexts.Games.UseCases.GetAll.DTOs;

public record PagedGameResponse(
    IEnumerable<SummaryGameResponse> Games,
    int Page,
    int PageSize,
    int Total);
