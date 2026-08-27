using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Games.ValueObjects;
using Flunt.Notifications;
using Flunt.Validations;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Create;

public static class Specification
{
    public static Contract<Notification> Ensure(Request request)
        => new Contract<Notification>()
            .Requires()
            
            // Title
            .IsNotNullOrWhiteSpace(request.Title, "Title", "Cannot be null or empty.")
            .IsGreaterThan(request.Title, 10, "Title", "Cannot be less than 10 characters.")
            .IsLowerThan(request.Title, GameTitle.MaxLength, "Title", $"Cannot be greater than {GameTitle.MaxLength}")
            
            // Description
            .IsNotNullOrWhiteSpace(request.Description, "Description", "Cannot be null or empty.")
            .IsGreaterThan(request.Description, 10, "Description", "Cannot be less than 10 characters.")
            .IsLowerThan(request.Description, 2000, "Description", "Cannot be greater than 2000 characters.")
            
            // Price (Garante que o preço não seja negativo antes de criar o Value Object)
            .IsGreaterOrEqualsThan(request.Price, 0, "Price", "Price must be greater than or equal to zero.")
            
            // Genre (Como é um Enum, validamos o valor recebido usando uma asserção booleana)
            .IsTrue(Enum.IsDefined(typeof(GameGenre), request.Genre), "Genre", "The provided game genre is invalid.")
            
            // ReleaseDate (Garante que a data de lançamento não está no passado)
            .IsTrue(
                request.ReleaseDate >= DateOnly.FromDateTime(DateTime.UtcNow), 
                "ReleaseDate", 
                "Release date cannot be in the past."
            );
}