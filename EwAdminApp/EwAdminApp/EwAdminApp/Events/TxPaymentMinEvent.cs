using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class TxPaymentMinEvent (TxPaymentMin? txPaymentMin)
{
    public TxPaymentMin? TxPaymentMin { get; set; }
}