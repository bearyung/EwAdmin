namespace EwAdminApi.Models.Pos;

public class ShopWorkdayDetail
{
 public int AccountId { get; set; }
 public int ShopId { get; set; }
 public int WorkdayDetailId { get; set; }
 public int WorkdayHeaderId { get; set; }
 public DateTime OpenDatetime { get; set; }
 public DateTime CloseDatetime { get; set; }
 public bool IsClosed { get; set; }
 public bool Enabled { get; set; }
 public DateTime? ModifiedDate { get; set; }
 public string? ModifiedBy { get; set; }
}