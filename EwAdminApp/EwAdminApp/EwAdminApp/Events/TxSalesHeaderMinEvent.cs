using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class TxSalesHeaderMinEvent(TxSalesHeaderMin? txSalesHeaderMinMessage)
{
    public TxSalesHeaderMin? TxSalesHeaderMinMessage { get; } = txSalesHeaderMinMessage;
}