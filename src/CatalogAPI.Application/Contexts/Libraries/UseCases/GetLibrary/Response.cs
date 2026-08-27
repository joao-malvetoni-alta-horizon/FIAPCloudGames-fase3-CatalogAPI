using CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary.DTOs;
using Flunt.Notifications;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.GetLibrary;

public class Response : Shared.Response
{
    #region Properties

    public PagedLibraryResponse? Library { get; }

    #endregion

    #region Constructors

    public Response(string message, PagedLibraryResponse? library)
    {
        Message =  message;
        Library = library;
        StatusCode = 200;
    }

    public Response(
        string message,
        int statusCode,
        IEnumerable<Notification>? notifications = null)
    {
        Message = message;
        StatusCode = statusCode;
        Notifications = notifications;
    }

    #endregion
}