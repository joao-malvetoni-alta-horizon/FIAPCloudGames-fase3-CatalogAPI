using CatalogAPI.Application.Contexts.Games.UseCases.GetAll.DTOs;
using Flunt.Notifications;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetAll;

public class Response : Shared.Response
{
    #region Properties

    public PagedGameResponse? Data { get; private set; }

    #endregion

    #region Constructors

    protected Response() { }

    public Response(string message, PagedGameResponse data)
    {
        Message = message;
        Data = data;
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