using MediatR;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetAll;

public record Request(int Page = 1, int PageSize = 20) : IRequest<Response>;