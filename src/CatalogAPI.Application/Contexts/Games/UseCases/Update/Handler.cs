using CatalogAPI.Application.Shared.Cache;
using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Update;

public class Handler(IUpdate repository, ICacheService cacheService) : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var cacheKey = $"games:{request.Id}";

        var res = Specification.Ensure(request);
        if (!res.IsValid)
            return new Response("Invalid request", 400, res.Notifications);

        var isSuccess = await repository
            .UpdateAsync(
                request.Id,
                request.Title,
                request.Description,
                request.Price,
                request.Genre,
                request.Status,
                request.ReleaseDate,
                cancellationToken);

        if (!isSuccess)
            throw new GameNotFoundException($"Game {request.Id} not found in the catalog.");

        await cacheService.RemoveAsync(cacheKey, cancellationToken);

        return new Response();
    }
}