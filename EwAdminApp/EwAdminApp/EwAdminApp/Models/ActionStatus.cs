namespace EwAdminApp.Models;

public class ActionStatus
{
    // enum for the status of the action
    public enum StatusEnum
    {
        Success,
        Error,
        Info,
        Executing,
        Completed,
        Interrupted
    }
    
    // properties
    public StatusEnum ActionStatusEnum { get; set; }
    public required string Message { get; set; }
}