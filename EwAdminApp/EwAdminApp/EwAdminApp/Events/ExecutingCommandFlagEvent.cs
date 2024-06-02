namespace EwAdminApp.Events;

public class ExecutingCommandFlagEvent(string sourceTypeName, bool isExecutionIncrement)
{
    public string SourceTypeName { get; } = sourceTypeName;
    public bool IsExecutionIncrement { get; } = isExecutionIncrement;
}