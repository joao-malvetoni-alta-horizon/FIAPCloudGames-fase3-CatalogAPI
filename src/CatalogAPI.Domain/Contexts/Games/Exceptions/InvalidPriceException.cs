using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Games.Exceptions;

public class InvalidPriceException(string message) : DomainException(message);
