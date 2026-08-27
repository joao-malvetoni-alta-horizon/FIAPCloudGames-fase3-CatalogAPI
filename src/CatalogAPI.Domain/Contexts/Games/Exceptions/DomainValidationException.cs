using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Games.Exceptions;

public class DomainValidationException(string message) : DomainException(message);
