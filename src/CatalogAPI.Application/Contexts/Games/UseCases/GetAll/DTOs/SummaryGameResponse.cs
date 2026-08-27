using CatalogAPI.Domain.Contexts.Games.Enums;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetAll.DTOs;

public record SummaryGameResponse(
    Guid Id,
    string Title,
    decimal Price,
    GameGenre Genre);
