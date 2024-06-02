using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class TxSalesHeaderEvent(TxSalesHeader? txSalesHeaderMessage)
{
    public TxSalesHeader? TxSalesHeaderMessage { get; } = txSalesHeaderMessage;
}