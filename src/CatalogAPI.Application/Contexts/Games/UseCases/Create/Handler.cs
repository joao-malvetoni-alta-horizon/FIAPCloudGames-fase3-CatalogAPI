using CatalogAPI.Application.Contexts.Games.UseCases.Create.DTOs;
using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Entities;
using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Create;

public class Handler(ICreate repository)
    : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var res = Specification.Ensure(request);
        if (!res.IsValid)
            return new Response("Invalid request", 400, res.Notifications);

        // Exceções de domínio (ex.: título/preço inválidos) e de infraestrutura propagam para o
        // GlobalExceptionHandler, que as traduz para o status HTTP adequado.
        var game = await repository.CreateAsync(
            new Game(
                request.Title,
                request.Description,
                request.Price,
                request.Genre,
                request.ReleaseDate),
            cancellationToken);

        return new Response(
            "Game created",
            new CreateGameResponse(
                game.Id,
                game.Title.Value,
                game.Description,
                game.Price.Amount,
                game.Genre)
            );
    }
}