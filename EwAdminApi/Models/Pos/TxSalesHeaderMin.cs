namespace EwAdminApi.Models.Pos;

public class TxSalesHeaderMin
{
    public int TxSalesHeaderId { get; set; }

    public int AccountId { get; set; }

    public int ShopId { get; set; }

    public string? TxCode { get; set; }

    public DateTime TxDate { get; set; }
    
    public bool Enabled { get; set; }

    public int TableId { get; set; }

    public string? TableCode { get; set; }
    
    public DateTime? CheckinDatetime { get; set; }

    public DateTime? CheckoutDatetime { get; set; }

    public int? CheckinUserId { get; set; }

    public string? CheckinUserName { get; set; }

    public int? CheckoutUserId { get; set; }

    public string? CheckoutUserName { get; set; }

    public int? CashierUserId { get; set; }

    public string? CashierUserName { get; set; }

    public DateTime? CashierDatetime { get; set; }

    public decimal? AmountPaid { get; set; }

    public decimal? AmountChange { get; set; }

    public decimal AmountSubtotal { get; set; }

    public decimal AmountServiceCharge { get; set; }

    public decimal AmountDiscount { get; set; }

    public decimal AmountTotal { get; set; }

    public decimal AmountRounding { get; set; }

    public bool TxCompleted { get; set; }

    public bool TxChecked { get; set; }

    public decimal? AmountTaxation { get; set; }

    public decimal? AmountMinChargeOffset { get; set; }

    public int? DisabledReasonId { get; set; }

    public string? DisabledReasonDesc { get; set; }

    public int? DisabledByUserId { get; set; }

    public string? DisabledByUserName { get; set; }

    public DateTime? DisabledDateTime { get; set; }
}