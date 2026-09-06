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

        // Observabilidade: marca a transação do New Relic com os identificadores do fluxo
        // "Compra de Jogo", para o trace ficar navegável e correlacionável com a PaymentsAPI.
        // Só identificadores opacos — nada de e-mail, token, CPF ou preço/dado de cobrança.
        AddPurchaseTraceAttributes(request.UserId, game.Id, orderPlaced.EventId);

        await eventPublisher.PublishAsync(orderPlaced, cancellationToken);

        return new Response("Processing order");
    }

    /// <summary>
    /// Anexa atributos customizados à transação corrente do agente APM. Fora de uma transação
    /// instrumentada (ex.: testes unitários), o agente devolve uma implementação no-op e a
    /// chamada é inofensiva. O <c>fcg.orderPlacedEventId</c> é a chave de correlação manual
    /// com a PaymentsAPI, já que o trace distribuído não atravessa o RabbitMQ (ver README).
    /// </summary>
    private static void AddPurchaseTraceAttributes(Guid userId, Guid gameId, Guid orderPlacedEventId)
    {
        var transaction = NewRelic.Api.Agent.NewRelic.GetAgent().CurrentTransaction;

        transaction.AddCustomAttribute("fcg.flow", "compra-jogo");
        transaction.AddCustomAttribute("fcg.userId", userId.ToString());
        transaction.AddCustomAttribute("fcg.gameId", gameId.ToString());
        transaction.AddCustomAttribute("fcg.orderPlacedEventId", orderPlacedEventId.ToString());
    }
}