using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Libraries.Exceptions;

public class UnauthorizedAccessException(string message) : DomainException(message);
