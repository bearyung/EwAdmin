using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class TxSalesHeaderMinEvent(TxSalesHeaderMin? txSalesHeaderMin)
{
    public TxSalesHeaderMin? TxSalesHeaderMin { get; } = txSalesHeaderMin;
}