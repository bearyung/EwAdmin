using EwAdmin.Common.Models.Pos;

namespace EwAdminApp.Events;

public class TxPaymentEvent(TxPayment? txPaymentMessage)
{
    public TxPayment? TxPaymentMessage { get; } = txPaymentMessage;
}