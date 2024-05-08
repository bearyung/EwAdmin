namespace EwAdminApp.Events;

public class MessageEvent(string message)
{
    public string Message { get; } = message;
}