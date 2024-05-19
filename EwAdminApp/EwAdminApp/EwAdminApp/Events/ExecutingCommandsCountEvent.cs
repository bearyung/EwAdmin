namespace EwAdminApp.Events;

public class ExecutingCommandsCountEvent(int count)
{
    public int ExecutingCommandsCount { get; } = count;
}