using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.ValueObjects;
using Flunt.Notifications;
using Flunt.Validations;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Update;

public static class Specification
{
    public static Contract<Notification> Ensure(Request request)
    {
        var contract = new Contract<Notification>();
        
        contract.AreNotEquals(request.Id, Guid.Empty, "Id", "Id is required.");
        
        if (request.Title is not null)
            contract
                .IsGreaterOrEqualsThan(request.Title, 3, "Title", "Title must contain at least 3 characters.")
                .IsLowerOrEqualsThan(request.Title, GameTitle.MaxLength, "Title", $"Cannot be greater than {GameTitle.MaxLength} characters.");

        if (request.Description is not null)
            contract
                .IsGreaterOrEqualsThan(request.Description, 10, "Description", "Description must contain at least 10 characters.")
                .IsLowerOrEqualsThan(request.Description, 500, "Description", "Description cannot be greater than 500 characters.");

        if (request.Price is not null)
            contract.IsGreaterOrEqualsThan(request.Price.Value, 0, "Price", "Price cannot be negative.");

        if (request.Genre is not null)
            contract.IsTrue(Enum.IsDefined(typeof(GameGenre), request.Genre.Value), "Genre", "Invalid game genre.");

        if (request.Status is not null)
            contract.IsTrue(Enum.IsDefined(typeof(GameStatus), request.Status.Value), "Status", "Invalid game status.");

        if (request.ReleaseDate is not null)
            contract.IsGreaterThan(request.ReleaseDate.Value.Year, 1950, "ReleaseDate", "Release date is too old.");

        return contract;
    }
}