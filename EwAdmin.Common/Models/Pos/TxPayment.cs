namespace EwAdmin.Common.Models.Pos;

public class TxPayment : TxPaymentMin
{
    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public string? OclNum { get; set; }

    public decimal? OclRemainValue { get; set; }

    public string? OclDeviceNum { get; set; }

    public string? RefNum { get; set; }

    public string? Remark1 { get; set; }

    public string? Remark2 { get; set; }

    public string? Remark3 { get; set; }

    public string? Remark4 { get; set; }

    public string? Remark5 { get; set; }

    public string? Remark6 { get; set; }

    public string? Remark7 { get; set; }

    public string? Remark8 { get; set; }

    public string? Remark9 { get; set; }

    public string? Remark10 { get; set; }

    public decimal? ChangeAmount { get; set; }

    public decimal? CashoutAmount { get; set; }

    public decimal? TipAmount { get; set; }

    public bool? IsDepositPayment { get; set; }

    public int? DepositReceivedByUserId { get; set; }

    public string? DepositReceivedByUserName { get; set; }

    public DateTime? DepositReceivedDatetime { get; set; }

    public int? DepositWorkdayDetailId { get; set; }

    public int? DepositWorkdayPeriodDetailId { get; set; }

    public string? DepositWorkdayPeriodName { get; set; }

    public decimal? PaymentMethodSurchargeAmount { get; set; }

    public decimal? PaymentMethodSurchargeRate { get; set; }

    public bool? IsNonSalesTxPayment { get; set; }

    public decimal? OverpaymentAmount { get; set; }

    public bool? IsPreprintedCouponTxPayment { get; set; }

    public string? PaymentRemark { get; set; }

    public string? PaymentCurrency { get; set; }

    public string? PaymentPathway { get; set; }

    public decimal? PaidAmountFx { get; set; }

    public decimal? ChangeAmountFx { get; set; }

    public bool? IsFxPayment { get; set; }

    public bool? IsChangeAmountFx { get; set; }

    public decimal? PaymentFxRate { get; set; }

    public decimal? TotalAmountFx { get; set; }

    public decimal? TxChargesRate { get; set; }

    public decimal? TxTotalCharges { get; set; }

    public decimal? TxTipCharges { get; set; }

    public decimal? TxNetTotal { get; set; }

    public decimal? TxNetTip { get; set; }

    public bool? IsOdoTx { get; set; }

    public bool? IsOnlinePayment { get; set; }
}