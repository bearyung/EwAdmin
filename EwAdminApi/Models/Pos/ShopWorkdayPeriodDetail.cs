namespace EwAdminApi.Models.Pos;

public class ShopWorkdayPeriodDetail
{
    // properties from ShopWorkdayPeriodDetail table
    public int WorkdayPeriodDetailId { get; set; }
    public int AccountId { get; set; }
    public int ShopId { get; set; }
    public int WorkdayDetailId { get; set; }
    public int WorkdayPeriodId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool Enabled { get; set; }
    
    // properties from ShopWorkdayPeriod table
    public string? PeriodName { get; set; }

}