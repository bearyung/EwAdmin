using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class TableMasterEvent(TableMaster? tableMasterMessage)
{
    public TableMaster? TableMasterMessage { get; } = tableMasterMessage;
}