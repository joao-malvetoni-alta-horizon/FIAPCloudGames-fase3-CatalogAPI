using CatalogAPI.Application.Shared.Messaging;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Domain.Contexts.Libraries.Exceptions;
using CatalogAPI.Domain.Contexts.Libraries.Queries;
using FiapCloudGames.Contracts.Catalog;
using MediatR;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.InitiateGamePurchase;

public class Handler(IGetGameAndCheckOwnership repository, IIntegrationEventPublisher eventPublisher)
    : IRequestHandler<Request, Response>
{
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var validation = Specification.Ensure(request);
        if (!validation.IsValid)
            return new Response("Invalid request", 400);

        var item = new LibraryItem(request.UserId, request.GameId);

        var (game, alreadyOwns) = await repository.ExecuteAsync(item, cancellationToken);

        if (game is null)
            throw new GameNotFoundException("Game not found in the catalog.");

        if (alreadyOwns)
            throw new GameAlreadyOwnedException("This game is already in your library.");

        var orderPlaced = new OrderPlacedEvent(request.UserId, game.Id, game.Price.Amount);
        await eventPublisher.PublishAsync(orderPlaced, cancellationToken);

        return new Response("Processing order");
    }
}