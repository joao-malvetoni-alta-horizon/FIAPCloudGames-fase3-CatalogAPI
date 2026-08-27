using CatalogAPI.Domain.Shared;

namespace CatalogAPI.Domain.Contexts.Games.Exceptions;

public sealed class InsufficientGameManagementPermissionException(string message)
    : DomainException(message);
