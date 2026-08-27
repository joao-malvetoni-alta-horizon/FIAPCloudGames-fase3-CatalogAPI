using CatalogAPI.Application.Contexts.Games.UseCases.Create.DTOs;
using Flunt.Notifications;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Create;

public class Response : Shared.Response
{
    #region Properties
    
    public CreateGameResponse? Game { get; private set; }
    
    #endregion
    
    #region Constructors
    
    protected Response() { }

    public Response(string message, CreateGameResponse game)
    {
        Message = message;
        Game = game;
        StatusCode = 201;
        Notifications = null;
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