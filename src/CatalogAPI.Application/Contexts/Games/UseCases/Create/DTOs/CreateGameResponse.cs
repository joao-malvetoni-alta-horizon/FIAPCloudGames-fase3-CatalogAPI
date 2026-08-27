using CatalogAPI.Domain.Contexts.Games.Enums;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Create.DTOs;

public record CreateGameResponse(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    GameGenre Genre);
