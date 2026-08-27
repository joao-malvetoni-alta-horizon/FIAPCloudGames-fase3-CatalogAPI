using Flunt.Notifications;

namespace CatalogAPI.Application.Contexts.Libraries.UseCases.InitiateGamePurchase;

public class Response : Shared.Response
{
    protected Response() {  }

    public Response(string message)
    {
        Message = message;
        StatusCode = 202;
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
}