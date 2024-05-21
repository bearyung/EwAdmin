using Avalonia.Media;

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
    
    public string? IconResourceKey => ActionStatusEnum switch
    {
        StatusEnum.Success => "IconCheckmarkCircleRegular",
        StatusEnum.Error => "IconDismissCircleRegular",
        StatusEnum.Info => "IconInfoRegular",
        StatusEnum.Executing => "IconArrowSyncCircleRegular",
        StatusEnum.Completed => "IconCheckmarkCircleRegular",
        StatusEnum.Interrupted => "IconDismissCircleRegular",
        _ => null
    };
    
    public SolidColorBrush IconColor => ActionStatusEnum switch
    {
        StatusEnum.Success => new SolidColorBrush(Colors.Green),
        StatusEnum.Error => new SolidColorBrush(Colors.Red),
        StatusEnum.Info => new SolidColorBrush(Colors.Transparent),
        StatusEnum.Executing => new SolidColorBrush(Colors.Orange),
        StatusEnum.Completed => new SolidColorBrush(Colors.Green),
        StatusEnum.Interrupted => new SolidColorBrush(Colors.Red),
        _ => new SolidColorBrush(Colors.Transparent)
    };
}