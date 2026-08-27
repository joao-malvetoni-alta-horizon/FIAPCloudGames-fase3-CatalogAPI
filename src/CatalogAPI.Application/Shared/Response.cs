using Flunt.Notifications;

namespace CatalogAPI.Application.Shared;

public abstract class Response
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 400;
    public bool IsSuccess => StatusCode is >= 200 and <= 299;
    public IEnumerable<Notification>? Notifications { get; set; }
}
