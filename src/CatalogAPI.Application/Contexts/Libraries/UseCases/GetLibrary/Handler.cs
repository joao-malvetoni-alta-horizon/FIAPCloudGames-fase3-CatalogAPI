using MediatR;
using CatalogAPI.Domain.Contexts.Libraries.Queries;
using CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary.DTOs;
using CatalogAPI.Application.Shared.Cache;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary;

public class Handler(IGetLibrary getLibraryQuery, ICacheService cacheService) : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var cacheKey = $"libraries:all:userId={request.UserId}:page={request.Page}:size={request.PageSize}";

        var res = Specification.Ensure(request);
        
        if (!res.IsValid)
        {
            return new Response(
                message: "Validation failed for the library request.",
                statusCode: 400,
                notifications: res.Notifications
            );
        }

        var pagedLibraryResponse  = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var(games, totalCount) = await getLibraryQuery.ExecuteAsync(
                    request.UserId,
                    request.Page,
                    request.PageSize,
                    cancellationToken
                );

                var gameDtos = games.Select(game => new LibraryGameItemResponse(
                    game.Id,
                    game.Title.Value,
                    game.Genre,
                    game.Price.Amount
                )).ToList();

                return new PagedLibraryResponse(
                    gameDtos,
                    request.Page,
                    request.PageSize,
                    totalCount
                );
            },
            TimeSpan.FromMinutes(3),
            cancellationToken
        );

        

        return new Response("User library retrieved successfully.", pagedLibraryResponse);
    }
}
