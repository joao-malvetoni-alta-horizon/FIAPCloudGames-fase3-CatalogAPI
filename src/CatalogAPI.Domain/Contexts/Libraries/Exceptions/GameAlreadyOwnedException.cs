using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Libraries.Exceptions;

public class GameAlreadyOwnedException(string message) : DomainException(message);
