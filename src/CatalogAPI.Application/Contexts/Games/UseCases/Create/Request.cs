using CatalogAPI.Domain.Contexts.Games.Enums;
using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Create;

public record Request(
    string Title,
    string Description,
    decimal Price,
    GameGenre Genre,
    DateOnly ReleaseDate) : IRequest<Response>;
