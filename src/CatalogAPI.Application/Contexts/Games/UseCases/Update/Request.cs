using CatalogAPI.Domain.Contexts.Games.Enums;
using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Update;

public record Request(
    Guid Id,
    string? Title,
    string? Description,
    decimal? Price,
    GameGenre? Genre,
    GameStatus? Status,
    DateOnly? ReleaseDate) : IRequest<Response>;
