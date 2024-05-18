using EwAdminApp.Models;

namespace EwAdminApp.Events;

public class ActionStatusMessageEvent(ActionStatus actionStatusMessage)
{
    public ActionStatus ActionStatusMessage { get; } = actionStatusMessage;
}