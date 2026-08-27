using CatalogAPI.Application.Contexts.Games.UseCases.GetById.DTOs;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Games.Queries;
using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetById;

public class Handler(IGetById repository) : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var res = Specification.Ensure(request);
        if (!res.IsValid)
            return new Response("Invalid request", 400, res.Notifications);

        var game = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (game is null)
            throw new GameNotFoundException($"Game {request.Id} not found in the catalog.");

        return new Response(
            "Success",
            new DetailGameResponse(
                game.Id,
                game.Title.Value,
                game.Description,
                game.Price.Amount,
                game.Genre,
                game.Status,
                game.ReleaseDate)
            );
    }
}