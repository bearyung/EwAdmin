namespace EwAdmin.Common.Models.Pos;

public class ReportTurnoverHeader
{
    public int ReportTurnoverHeaderId { get; set; }

    public int AccountId { get; set; }

    public int ShopId { get; set; }

    public DateTime ClearingDatetime { get; set; }

    public DateTime LastPrintDatetime { get; set; }

    public int LastPrintCount { get; set; }

    public string? LastPrintBy { get; set; }

    public decimal DayTurnover { get; set; }

    public decimal DayDiscount { get; set; }

    public decimal DayServiceFee { get; set; }

    public decimal DayRounding { get; set; }

    public decimal DayNetTurnoverAmount { get; set; }

    public int DayNetTurnoverTxCount { get; set; }

    public int UncloseTxCount { get; set; }

    public decimal UncloseTxAmount { get; set; }

    public int WorkdayDetailId { get; set; }

    public DateTime WorkdayOpenDatetime { get; set; }

    public DateTime WorkdayCloseDatetime { get; set; }

    public bool Enabled { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }
}