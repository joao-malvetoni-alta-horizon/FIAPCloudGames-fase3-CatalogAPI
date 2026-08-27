using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Games.Exceptions;

public class GameNotFoundException(string message) : DomainException(message);
