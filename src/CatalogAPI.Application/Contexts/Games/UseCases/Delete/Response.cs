using Flunt.Notifications;

namespace CatalogAPI.Application.Contexts.Games.UseCases.Delete;

public class Response : Shared.Response
{
    
    #region Constructors

    public Response()
    {
        StatusCode = 204;
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