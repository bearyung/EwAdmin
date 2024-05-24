namespace EwAdmin.Common.Models.WebAdmin;

public class BrandMaster
{
    public int BrandId { get; set; }
    public string? BrandName { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int RegionId { get; set; }
    public string? RegionName { get; set; }
}