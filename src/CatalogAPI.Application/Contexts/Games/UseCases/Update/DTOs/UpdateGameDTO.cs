using CatalogAPI.Domain.Contexts.Games.Enums;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Update.DTOs;

public record UpdateGameDTO(
    string? Title,
    string? Description,
    decimal? Price,
    GameGenre? Genre,
    GameStatus? Status,
    DateOnly? ReleaseDate);
