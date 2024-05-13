using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class TxPaymentMinEvent(TxPaymentMin? txPaymentMinMessage)
{
    public TxPaymentMin? TxPaymentMinMessage { get; } = txPaymentMinMessage;

}