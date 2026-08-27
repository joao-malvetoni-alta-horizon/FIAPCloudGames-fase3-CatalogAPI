using MediatR;
using CatalogAPI.Domain.Contexts.Libraries.Queries;
using CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary.DTOs;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary;

public class Handler : IRequestHandler<Request, Response>
{
    private readonly IGetLibrary _getLibraryQuery;

    public Handler(IGetLibrary getLibraryQuery)
    {
        _getLibraryQuery = getLibraryQuery;
    }

    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var res = Specification.Ensure(request);
        
        if (!res.IsValid)
        {
            return new Response(
                message: "Validation failed for the library request.",
                statusCode: 400,
                notifications: res.Notifications
            );
        }

        var (games, totalCount) = await _getLibraryQuery.ExecuteAsync(
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

        var pagedLibraryResponse = new PagedLibraryResponse(
            gameDtos,
            request.Page,
            request.PageSize,
            totalCount
        );

        return new Response("User library retrieved successfully.", pagedLibraryResponse);
    }
}
