using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Delete;

public class Handler(IDelete repository) : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var res = Specification.Ensure(request);
        if (!res.IsValid)
            return new Response("Invalid request", 400, res.Notifications);

        var isSuccess = await repository.DeleteAsync(request.Id, cancellationToken);

        if (!isSuccess)
            throw new GameNotFoundException($"Game {request.Id} not found in the catalog.");

        return new Response();
    }
}