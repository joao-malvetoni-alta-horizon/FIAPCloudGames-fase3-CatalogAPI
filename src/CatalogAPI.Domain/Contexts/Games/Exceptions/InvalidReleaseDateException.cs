using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Games.Exceptions;

public class InvalidReleaseDateException(string message) : DomainException(message);
