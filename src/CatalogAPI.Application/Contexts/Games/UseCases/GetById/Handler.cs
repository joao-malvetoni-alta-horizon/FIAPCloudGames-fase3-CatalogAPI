using CatalogAPI.Application.Contexts.Games.UseCases.GetById.DTOs;
using CatalogAPI.Application.Shared.Cache;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Games.Queries;
using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetById;

public class Handler(IGetById repository, ICacheService cacheService) : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var cacheKey = $"games:{request.Id}";

        var res = Specification.Ensure(request);
        if (!res.IsValid)
            return new Response("Invalid request", 400, res.Notifications);

        var dto = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var game = await repository.GetByIdAsync(request.Id, cancellationToken);

                return game is null
                ? null
                : new DetailGameResponse(
                    game.Id,
                    game.Title.Value,
                    game.Description,
                    game.Price.Amount,
                    game.Genre,
                    game.Status,
                    game.ReleaseDate);
            },
            TimeSpan.FromMinutes(20),
            cancellationToken
        );

        if (dto is null)
            throw new GameNotFoundException($"Game {request.Id} not found in the catalog.");

        return new Response(
            "Success",
            dto
        );
    }
}