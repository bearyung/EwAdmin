namespace EwAdmin.Common.Models.Pos;

public class TableMaster
{
    public int TableId { get; set; }

    public int AccountId { get; set; }

    public int ShopId { get; set; }

    public string? TableCode { get; set; }

    public int SectionId { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAlt { get; set; }

    public int TableTypeId { get; set; }

    public int TableStatusId { get; set; }

    public bool IsTakeAway { get; set; }

    public bool IsTempTable { get; set; }

    public bool Enabled { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public int? DisplayIndex { get; set; }

    public int? ParentTableId { get; set; }

    public bool? IsAppearOnFloorPlan { get; set; }

    public int? SeatNum { get; set; }
    
    // from tableSection table's columns
    public string? SectionName { get; set; }
    
}