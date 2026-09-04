using CatalogAPI.Application.Contexts.Games.UseCases.GetAll.DTOs;
using CatalogAPI.Application.Shared.Cache;
using CatalogAPI.Domain.Contexts.Games.Queries;
using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetAll;

public class Handler(IGetAll repository, ICacheService cacheService) : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var cacheKey = $"games:all:page={request.Page}:size={request.PageSize}";

        var res = Specification.Ensure(request);
        if (!res.IsValid)
            return new Response("Invalid request", 400, res.Notifications);

        var (games, total) = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                return await repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
            },
            TimeSpan.FromMinutes(3),
            cancellationToken
        );

        var gamesFormatted = games.Select(game =>
            new SummaryGameResponse(game.Id, game.Title.Value, game.Price.Amount, game.Genre)
        );

        return new Response("Success", new PagedGameResponse(gamesFormatted, request.Page, request.PageSize, total));
    }
}