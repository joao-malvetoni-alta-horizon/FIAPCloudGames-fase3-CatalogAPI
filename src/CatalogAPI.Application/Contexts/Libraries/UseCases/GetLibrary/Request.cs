using MediatR;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary;

public record Request(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<Response>;
