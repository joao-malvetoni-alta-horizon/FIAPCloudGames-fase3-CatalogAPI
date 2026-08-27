using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Delete;

public record Request(Guid Id) : IRequest<Response>;
