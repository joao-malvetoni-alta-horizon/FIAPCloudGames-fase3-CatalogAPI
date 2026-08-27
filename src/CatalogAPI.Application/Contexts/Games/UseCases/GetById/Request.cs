using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetById;

public record Request(Guid Id) : IRequest<Response>;
