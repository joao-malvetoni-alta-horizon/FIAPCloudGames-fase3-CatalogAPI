using CatalogAPI.Application.Contexts.Games.UseCases.GetById.DTOs;
using Flunt.Notifications;

namespace CatalogAPI.Application.Contexts.Games.UseCases.GetById;

public class Response : Shared.Response
{
    #region Properties

    public DetailGameResponse? Game { get; private set; }

    #endregion
    
    #region Constructors

    protected Response() { }

    public Response(string message, DetailGameResponse game)
    {
        Message = message;
        StatusCode = 200;
        Game = game;
    }
    public Response(
        string message, int statusCode, IEnumerable<Notification>? notifications = null)
    {
        Message = message;
        StatusCode = statusCode;
        Notifications = notifications;
    }

    #endregion
}