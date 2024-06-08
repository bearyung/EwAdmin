using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class ReportTurnoverHeaderEvent(ReportTurnoverHeader? reportTurnoverHeaderMessage)
{
    public ReportTurnoverHeader? ReportTurnoverHeaderMessage { get; } = reportTurnoverHeaderMessage;
}