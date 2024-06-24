namespace EwAdmin.Common.Models.Pos;

public class ItemCategoryShopDetail
{
    public int CategoryId { get; set; }
 
    public int ShopId { get; set; }
 
    public int AccountId { get; set; }
 
    public string? DisplayName { get; set; }
 
    public int? DisplayIndex { get; set; }
 
    public bool IsPublicDisplay { get; set; }
 
    public bool Enabled { get; set; }
 
    public DateTime CreatedDate { get; set; }
 
    public string? CreatedBy { get; set; }
 
    public DateTime ModifiedDate { get; set; }
 
    public string? ModifiedBy { get; set; }
}