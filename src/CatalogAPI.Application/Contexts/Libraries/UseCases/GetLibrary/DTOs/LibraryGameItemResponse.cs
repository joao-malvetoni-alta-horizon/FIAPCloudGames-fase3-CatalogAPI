using CatalogAPI.Domain.Contexts.Games.Enums;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary.DTOs;

public record LibraryGameItemResponse(
    Guid GameId,
    string Title,
    GameGenre Genre,
    decimal Price);
