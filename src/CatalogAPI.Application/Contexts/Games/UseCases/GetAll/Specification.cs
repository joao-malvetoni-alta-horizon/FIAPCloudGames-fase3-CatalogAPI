using Flunt.Notifications;
using Flunt.Validations;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetAll;

public static class Specification
{
    public static Contract<Notification> Ensure(Request request)
        => new Contract<Notification>()
            .IsGreaterThan(request.Page, 0, "Page", "Page must be greater than 0")
            .IsGreaterThan(request.PageSize, 0, "PageSize", "PageSize must be greater than 0")
            .IsLowerOrEqualsThan(request.PageSize, 50, "PageSize", "PageSize cannot be greater than 50");
}
