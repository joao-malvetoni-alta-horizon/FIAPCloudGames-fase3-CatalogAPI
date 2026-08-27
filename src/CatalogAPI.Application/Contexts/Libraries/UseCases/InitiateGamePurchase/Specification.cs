using Flunt.Notifications;
using Flunt.Validations;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.InitiateGamePurchase;

public static class Specification
{
    public static Contract<Notification> Ensure(Request request)
        => new Contract<Notification>()
            .Requires()
            .AreNotEquals(request.UserId, Guid.Empty, "UserId", "Cannot be empty")
            .AreNotEquals(request.GameId, Guid.Empty, "GameId", "Cannot be empty");
}