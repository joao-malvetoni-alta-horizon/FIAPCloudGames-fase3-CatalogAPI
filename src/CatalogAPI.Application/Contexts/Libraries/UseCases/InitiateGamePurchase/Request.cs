using MediatR;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.InitiateGamePurchase;

public record Request(Guid UserId, Guid GameId) : IRequest<Response>;
