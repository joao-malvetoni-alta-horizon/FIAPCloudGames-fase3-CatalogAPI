using Flunt.Notifications;
using Flunt.Validations;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetById;

public static class Specification
{
    public static Contract<Notification> Ensure(Request request)
        => new Contract<Notification>()
            .AreNotEquals(request.Id, Guid.Empty, "Id", "Id is required");
}
