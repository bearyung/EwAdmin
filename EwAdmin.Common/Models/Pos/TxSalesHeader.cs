namespace EwAdmin.Common.Models.Pos;

public class TxSalesHeader : TxSalesHeaderMin
{
    public string? ReceiptNo { get; set; }

    public bool IsCurrentTx { get; set; }

    public bool Voided { get; set; }
    
    public int? PreviousTableId { get; set; }

    public string? PreviousTableCode { get; set; }

    public int SectionId { get; set; }

    public string? SectionName { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsTakeAway { get; set; }

    public int? TakeAwayRunningIndex { get; set; }

    public int? WorkdayPeriodDetailId { get; set; }

    public string? WorkdayPeriodName { get; set; }

    public int? DiscountId { get; set; }

    public string? DiscountName { get; set; }

    public string? CashDrawerCode { get; set; }

    public int ReceiptPrintCount { get; set; }

    public int TxRevokeCount { get; set; }

    public int? ServiceChargeId { get; set; }

    public string? ServiceChargeName { get; set; }

    public decimal AmountTips { get; set; }

    public bool? IsTimeLimited { get; set; }

    public int? TimeLimitedMinutes { get; set; }

    public int? CusCount { get; set; }

    public int? DiscountByUserId { get; set; }

    public string? DiscountByUserName { get; set; }

    public decimal? AmountPointTotal { get; set; }

    public decimal? MemberPointRemain { get; set; }

    public int? TaxationId { get; set; }

    public string? TaxationName { get; set; }


    public bool? IsMinChargeOffsetWaived { get; set; }

    public bool? IsMinChargeTx { get; set; }

    public bool? IsMinChargePerHead { get; set; }

    public decimal? MinChargeAmount { get; set; }

    public decimal? MinChargeMemberAmount { get; set; }

    public bool? IsPrepaidRechargeTx { get; set; }

    public bool? IsInvoicePrintPending { get; set; }

    public int? InvoiceNum { get; set; }

    public int? OrderNum { get; set; }

    public bool? IsDepositTx { get; set; }

    public decimal? TotalDepositAmount { get; set; }

    public string? DepositRemark { get; set; }

    public bool? IsDepositOutstanding { get; set; }

    public bool? IsReturnTx { get; set; }

    public bool? HasReturned { get; set; }

    public DateTime? ReturnedDateTime { get; set; }

    public int? ReturnedTxSalesHeaderId { get; set; }

    public int? NewTxSalesHeaderIdForReturn { get; set; }

    public int? ApiGatewayRefId { get; set; }

    public string? ApiGatewayName { get; set; }

    public string? ApiGatewayRefRemark { get; set; }

    public string? TableRemark { get; set; }

    public string? TxSalesHeaderRemark { get; set; }

    public decimal? TotalPaymentMethodSurchargeAmount { get; set; }

    public bool? IsNonSalesTx { get; set; }

    public bool? IsNoOtherLoyaltyTx { get; set; }

    public int? StartWorkdayPeriodDetailId { get; set; }

    public string? StartWorkdayPeriodName { get; set; }

    public bool? IsTxOnHold { get; set; }

    public Guid? OdoOrderToken { get; set; }

    public decimal? AmountOverpayment { get; set; }

    public int? TxStatusId { get; set; }

    public string? OverridedChecklistPrinterName { get; set; }

    public int? OrderSourceTypeId { get; set; }

    public int? OrderSourceRefId { get; set; }

    public int? OrderChannelId { get; set; }

    public string? OrderChannelCode { get; set; }

    public string? OrderChannelName { get; set; }

    public string? ApiGatewayRefCode { get; set; }

    public string? ApiGatewayResponseCode { get; set; }
}