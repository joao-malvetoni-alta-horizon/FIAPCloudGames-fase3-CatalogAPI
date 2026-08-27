using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Games.Exceptions;

public class InvalidGameTitleException(string message) : DomainException(message);
