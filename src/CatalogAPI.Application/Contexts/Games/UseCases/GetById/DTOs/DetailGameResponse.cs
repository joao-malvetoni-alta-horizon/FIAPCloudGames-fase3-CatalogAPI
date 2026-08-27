using CatalogAPI.Domain.Contexts.Games.Enums;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetById.DTOs;

public record DetailGameResponse(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    GameGenre Genre,
    GameStatus Status,
    DateOnly ReleaseDate);
