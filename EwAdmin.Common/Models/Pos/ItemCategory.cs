namespace EwAdmin.Common.Models.Pos;

public class ItemCategory
{
    public int CategoryId { get; set; }
 
    public int AccountId { get; set; }
 
    public string? CategoryName { get; set; }
 
    public string? CategoryNameAlt { get; set; }
 
    public int DisplayIndex { get; set; }
 
    public int? ParentCategoryId { get; set; }
 
    public bool IsTerminal { get; set; }
 
    public bool IsPublicDisplay { get; set; }
 
    public int? ButtonStyleId { get; set; }
 
    public string? PrinterName { get; set; }
 
    public bool IsModifier { get; set; }
 
    public bool Enabled { get; set; }
 
    public DateTime CreatedDate { get; set; }
 
    public string? CreatedBy { get; set; }
 
    public DateTime ModifiedDate { get; set; }
 
    public string? ModifiedBy { get; set; }
 
    public string? PrinterName2 { get; set; }
 
    public string? PrinterName3 { get; set; }
 
    public string? PrinterName4 { get; set; }
 
    public string? PrinterName5 { get; set; }
 
    public int? CategoryTypeId { get; set; }
 
    public string? ImageFileName { get; set; }
 
    public string? ImageFileName2 { get; set; }
 
    public string? ImageFileName3 { get; set; }
 
    public bool? IsSelfOrderingDisplay { get; set; }
 
    public bool? IsOnlineStoreDisplay { get; set; }
 
    public string? CategoryCode { get; set; }
}