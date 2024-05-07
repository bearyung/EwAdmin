namespace EwAdminApi.Models.Pos;

public class TxPaymentMin
{
    public int TxPaymentId { get; set; }

    public int AccountId { get; set; }

    public int ShopId { get; set; }

    public int TxSalesHeaderId { get; set; }

    public int PaymentMethodId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public bool Enabled { get; set; }

    public string? PaymentMethodCode { get; set; }

    public string? PaymentMethodName { get; set; }
}